using AppInfoGUI.Package.Streams;
using System.IO;

namespace AppInfoGUI.Package.Nsis;

// The files an installer carries, as a stream of bytes per file. Nothing is written anywhere.
// There are two ways NSIS stores them, so there are two kinds of payload.
public abstract class NsisPayload
{
    // Calls `visit` once for each distinct piece of data, with its bytes and its length when that is known.
    // The bytes must be read inside `visit`, in one go.
    public abstract void Visit(IEnumerable<NsisFile> files, Action<NsisFile, Stream, long?> visit);

    public static NsisPayload Create(Stream file, NsisFirstHeader first, NsisHeaderData header) =>
        header.FilesStart is long start
            ? new NsisSeparatePayload(file, start)
            : new NsisSolidPayload(file, first);

    // One entry per distinct data offset, in the order they sit in the data
    protected static List<NsisFile> Distinct(IEnumerable<NsisFile> files) =>
        files.GroupBy(f => f.DataOffset).Select(g => g.First()).OrderBy(f => f.DataOffset).ToList();
}

// Each file is packed (or not) on its own and can be reached directly: [length, top bit = packed][data]
public class NsisSeparatePayload : NsisPayload
{
    const uint PackedFlag = 0x80000000;

    readonly Stream _file;
    readonly long _filesStart;

    public NsisSeparatePayload(Stream file, long filesStart)
    {
        _file = file;
        _filesStart = filesStart;
    }

    public override void Visit(IEnumerable<NsisFile> files, Action<NsisFile, Stream, long?> visit)
    {
        foreach (NsisFile item in Distinct(files))
        {
            long at = _filesStart + item.DataOffset;
            if (at < 0 || at + 4 > _file.Length) continue;

            var lead = new byte[4];
            _file.Position = at;
            _file.ReadExactly(lead, 0, 4);

            uint word = BitConverter.ToUInt32(lead);
            long length = word & ~PackedFlag;
            if (at + 4 + length > _file.Length) continue;

            var bytes = new SubStream(_file, at + 4, length);
            if ((word & PackedFlag) == 0)
            {
                visit(item, bytes, length);
                continue;
            }

            using Stream unpacked = NsisCompression.Open(bytes);
            visit(item, unpacked, null);
        }
    }
}

// The whole data is one packed stream. The files follow the script header, each as [length][data].
public class NsisSolidPayload : NsisPayload
{
    readonly Stream _file;
    readonly NsisFirstHeader _first;

    public NsisSolidPayload(Stream file, NsisFirstHeader first)
    {
        _file = file;
        _first = first;
    }

    public override void Visit(IEnumerable<NsisFile> files, Action<NsisFile, Stream, long?> visit)
    {
        long length = Math.Min(_first.DataLength, _file.Length - _first.DataStart);
        using Stream unpacked = NsisCompression.Open(new SubStream(_file, _first.DataStart, length));

        Skip(unpacked, 4 + (long)_first.HeaderLength);   // the header's length, then the header
        long position = 0;                                // counted from the start of the files

        foreach (NsisFile item in Distinct(files))
        {
            if (item.DataOffset < position) continue;
            Skip(unpacked, item.DataOffset - position);

            var lead = new byte[4];
            unpacked.ReadExactly(lead, 0, 4);
            long size = BitConverter.ToUInt32(lead);

            var bytes = new LimitedStream(unpacked, size);
            visit(item, bytes, size);
            Skip(unpacked, size - bytes.Taken);       // whatever `visit` did not read

            position = item.DataOffset + 4 + size;
        }
    }

    // The packed stream cannot jump ahead, so the bytes are read and thrown away
    static void Skip(Stream stream, long count)
    {
        var buffer = new byte[81920];
        while (count > 0)
        {
            int read = stream.Read(buffer, 0, (int)Math.Min(buffer.Length, count));
            if (read == 0) throw new InvalidDataException("the NSIS data ended too early");
            count -= read;
        }
    }
}
