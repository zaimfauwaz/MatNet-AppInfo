using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Common;
using System.IO;

namespace AppInfoGUI.Package;

// Lists the files in a 7-Zip or RAR archive that sits behind a program (a self-extracting archive).
// SharpCompress reads the archive's file list only. Nothing is unpacked.
internal class ArchiveLister : IPackageLister
{
    const int SearchWindow = 128 * 1024;   // same window the detector looked through

    readonly byte[] _signature;
    readonly Func<Stream, IArchive> _open;

    ArchiveLister(byte[] signature, Func<Stream, IArchive> open)
    {
        _signature = signature;
        _open = open;
    }

    public static ArchiveLister SevenZip() => new(Signatures.SevenZip, s => SevenZipArchive.OpenArchive(s));
    public static ArchiveLister Rar() => new(Signatures.Rar, s => RarArchive.OpenArchive(s));

    public List<PackageEntry> List(Stream file, Overlay overlay)
    {
        long start = overlay.Start + FindSignature(file, overlay);
        using var window = new SubStream(file, start, overlay.End - start);

        try
        {
            using IArchive archive = _open(window);
            return archive.Entries
                .Where(e => !e.IsDirectory)
                .Select(e => new PackageEntry(e.Key ?? "", e.Size, EntryClassifier.Classify(e.Key ?? ""), e.IsEncrypted))
                .ToList();
        }
        catch (SharpCompressException)
        {
            // Usually a password-protected file list. Nothing else is expected to fail here.
            throw new InvalidDataException("the list of files could not be read (it may be password protected)");
        }
    }

    // Offset of the archive signature inside the extra data
    int FindSignature(Stream file, Overlay overlay)
    {
        var buffer = new byte[(int)Math.Min(overlay.Length, SearchWindow)];
        file.Position = overlay.Start;
        file.ReadExactly(buffer, 0, buffer.Length);

        int index = buffer.AsSpan().IndexOf(_signature);
        if (index < 0) throw new InvalidDataException("the archive start was not found");
        return index;
    }
}
