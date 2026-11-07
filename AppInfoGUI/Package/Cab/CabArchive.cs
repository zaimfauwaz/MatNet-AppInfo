using System.IO;
using System.Text;

namespace AppInfoGUI.Package.Cab;

public record CabEntry(string Name, int Size, int Folder, int Offset);

// Reads a cabinet (.cab) that sits in a stream. The list of files is read from its directory.
// A file's bytes are unpacked in memory only when asked for.
public class CabArchive
{
    const int MaxFileSize = 64 * 1024 * 1024;
    const int StoredType = 0;
    const int MsZipType = 1;
    const int LzxType = 3;

    record Folder(long FirstBlock, int BlockCount, int Type);

    readonly Stream _stream;
    readonly long _start;
    readonly int _blockReserve;
    readonly List<Folder> _folders = new();

    public List<CabEntry> Entries { get; } = new();

    public CabArchive(Stream stream, long start)
    {
        _stream = stream;
        _start = start;

        stream.Position = start;
        var header = new byte[36];
        stream.ReadExactly(header, 0, 36);
        if (header[0] != 'M' || header[1] != 'S' || header[2] != 'C' || header[3] != 'F')
            throw new InvalidDataException("the cabinet was not found");

        int filesAt = BitConverter.ToInt32(header, 16);
        int folderCount = BitConverter.ToUInt16(header, 26);
        int fileCount = BitConverter.ToUInt16(header, 28);
        int flags = BitConverter.ToUInt16(header, 30);

        int folderReserve = 0;
        if ((flags & 4) != 0)
        {
            var reserve = new byte[4];
            stream.ReadExactly(reserve, 0, 4);
            folderReserve = reserve[2];
            _blockReserve = reserve[3];
            stream.Position += BitConverter.ToUInt16(reserve, 0);
        }

        if ((flags & 3) != 0) throw new NotSupportedException("cabinets in several parts are not supported");

        for (int i = 0; i < folderCount; i++)
        {
            var folder = new byte[8];
            stream.ReadExactly(folder, 0, 8);
            stream.Position += folderReserve;
            _folders.Add(new Folder(start + BitConverter.ToInt32(folder, 0), BitConverter.ToUInt16(folder, 4), BitConverter.ToUInt16(folder, 6)));
        }

        stream.Position = start + filesAt;
        for (int i = 0; i < fileCount; i++)
        {
            var file = new byte[16];
            stream.ReadExactly(file, 0, 16);
            Entries.Add(new CabEntry(ReadName(stream), BitConverter.ToInt32(file, 0), BitConverter.ToUInt16(file, 8), BitConverter.ToInt32(file, 4)));
        }
    }

    // The unpacked bytes of one file
    public byte[] Read(CabEntry entry)
    {
        if (entry.Size > MaxFileSize) throw new NotSupportedException("a file in the cabinet is too big to read");
        if (entry.Folder >= _folders.Count) throw new InvalidDataException("the cabinet is damaged");

        Folder folder = _folders[entry.Folder];
        ICabDecoder decoder = DecoderFor(folder.Type);
        var output = new GrowingBuffer();
        int needed = entry.Offset + entry.Size;

        // The blocks of a folder are packed one after the other, so they are read from the first one
        _stream.Position = folder.FirstBlock;
        for (int i = 0; i < folder.BlockCount && output.Length < needed; i++)
        {
            var header = new byte[8];
            _stream.ReadExactly(header, 0, 8);
            int packed = BitConverter.ToUInt16(header, 4);
            int size = BitConverter.ToUInt16(header, 6);
            _stream.Position += _blockReserve;

            var block = new byte[packed];
            _stream.ReadExactly(block, 0, packed);
            decoder.Decode(block, size, output);
        }

        if (output.Length < needed) throw new InvalidDataException("the cabinet is damaged");
        return output.ToArray(entry.Offset, entry.Size);
    }

    static ICabDecoder DecoderFor(int type) => (type & 0xF) switch
    {
        StoredType => new StoredDecoder(),
        MsZipType => new MsZipDecoder(),
        LzxType => new LzxDecoder((type >> 8) & 0x1F),
        _ => throw new NotSupportedException("this way of packing the cabinet is not supported")
    };

    static string ReadName(Stream stream)
    {
        var bytes = new List<byte>();
        for (int b; (b = stream.ReadByte()) > 0;) bytes.Add((byte)b);
        return Encoding.UTF8.GetString(bytes.ToArray());
    }
}
