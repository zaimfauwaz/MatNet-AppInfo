using AppInfoGUI.Package.Archives;
using System.IO;

namespace AppInfoGUI.Package.Nsis;

// Many installers carry their real program as one archive (often a .7z) and unpack it with a plugin.
// This lists what is inside such an archive. Only an archive stored as it is can be reached.
public static class NsisNested
{
    const uint PackedFlag = 0x80000000;

    // The rows for the inside of `item`, or none when it is not an archive
    public static List<PackageEntry> List(Stream file, long? filesStart, NsisFile item)
    {
        if (!NestedArchive.IsArchive(item.Path)) return new();

        if (filesStart == null)
            return Note("this installer is one solid packed stream, so archives inside it are not read yet");

        // The file's data starts with its length. The top bit says the data is packed.
        long at = filesStart.Value + item.DataOffset;
        var lead = new byte[4];
        file.Position = at;
        file.ReadExactly(lead, 0, 4);

        uint word = BitConverter.ToUInt32(lead);
        if ((word & PackedFlag) != 0) return Note("this archive is packed by the installer, so it is not read yet");

        return NestedArchive.List(file, item.Path, at + 4, word);
    }

    static List<PackageEntry> Note(string text) => new() { NestedArchive.Note($"({text})") };
}
