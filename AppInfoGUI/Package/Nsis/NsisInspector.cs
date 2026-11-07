using AppInfoGUI.Package.Archives;
using AppInfoGUI.Package.Streams;
using System.IO;

namespace AppInfoGUI.Package.Nsis;

// Goes through the installer's data once, to learn each file's size and to list the archives inside.
// The bytes are read and counted, never stored: for an archive only its start and end are kept in memory.
public static class NsisInspector
{
    // The list of files of a 7z or a zip sits at its end. This much of the end is kept.
    const int HeadSize = 256 * 1024;     // the archive lister looks for the archive's mark in the first 128 KB
    const int TailSize = 16 * 1024 * 1024;

    // Info per data offset. `failure` is set when the data could not be read to the end.
    public static Dictionary<int, NsisFileInfo> Inspect(NsisPayload payload, List<NsisFile> files, out string? failure)
    {
        var infos = new Dictionary<int, NsisFileInfo>();
        failure = null;

        try
        {
            payload.Visit(files, (item, bytes, length) => infos[item.DataOffset] = Look(item, bytes, length));
        }
        catch (Exception ex) when (ex is IOException or InvalidDataException or NotSupportedException)
        {
            failure = ex.Message;
        }

        return infos;
    }

    static NsisFileInfo Look(NsisFile item, Stream bytes, long? length)
    {
        if (!NestedArchive.IsArchive(item.Path))
            return new NsisFileInfo(length ?? Count(bytes), new());

        // A file stored as it is can be read in place
        if (length != null && bytes.CanSeek)
            return new NsisFileInfo(length.Value, NestedArchive.List(bytes, item.Path, 0, length.Value));

        var recorder = new HeadTailRecorder(HeadSize, TailSize);
        bytes.CopyTo(recorder);
        return new NsisFileInfo(recorder.Total, NestedArchive.List(recorder.ToReadable(), item.Path, 0, recorder.Total));
    }

    static long Count(Stream bytes)
    {
        var buffer = new byte[81920];
        long total = 0;
        for (int read; (read = bytes.Read(buffer, 0, buffer.Length)) > 0;) total += read;
        return total;
    }
}
