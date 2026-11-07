using System.IO;

namespace AppInfoGUI.Info;

internal static class FileFlags
{
    public static string Attributes(string path)
    {
        FileAttributes attributes = File.GetAttributes(path);
        var flags = new List<string>();

        if (attributes.HasFlag(FileAttributes.Hidden)) flags.Add("Hidden");
        if (attributes.HasFlag(FileAttributes.System)) flags.Add("System file");
        if (attributes.HasFlag(FileAttributes.ReadOnly)) flags.Add("Read-only");

        return flags.Count == 0 ? "Normal" : string.Join(", ", flags);
    }

    // A file changed before it was created is normal for copies, so this is only a note
    public static string Timestamps(FileInfo file) =>
        file.LastWriteTime < file.CreationTime
            ? "Modified is earlier than Created (normal for copied files)"
            : "Consistent";
}
