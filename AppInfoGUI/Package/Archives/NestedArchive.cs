using System.IO;

namespace AppInfoGUI.Package.Archives;

// Lists an archive (.7z, .zip, .rar) that sits inside a package, as one more level of rows.
// Used by every package format that can reach the bytes of a file it holds.
public static class NestedArchive
{
    // Null when the file name is not an archive we can read
    static IPackageLister? ListerFor(string name) => System.IO.Path.GetExtension(name).ToLowerInvariant() switch
    {
        ".7z" => ArchiveLister.SevenZip(),
        ".rar" => ArchiveLister.Rar(),
        ".zip" => new ZipLister(),
        _ => null
    };

    public static bool IsArchive(string name) => ListerFor(name) != null;

    // The archive's bytes are the part of `file` from `start` for `length` bytes
    public static List<PackageEntry> List(Stream file, string name, long start, long length)
    {
        try
        {
            IPackageLister lister = ListerFor(name)!;
            return lister.List(file, new Overlay(start, start + length)).Select(e => e with { Depth = 1 }).ToList();
        }
        catch (Exception ex)
        {
            return new List<PackageEntry> { Note($"(could not be listed: {ex.Message})") };
        }
    }

    // A row that says why the inside of an archive is not shown
    public static PackageEntry Note(string text) => new(text, -1, EntryKind.Note, false, Depth: 1);
}
