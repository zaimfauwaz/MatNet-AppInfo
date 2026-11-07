using AppInfoGUI.Info;

namespace AppInfoGUI.Installer;

internal static class FileEntries
{
    public static IEnumerable<FileEntry> Read(MsiPackage package)
    {
        var folders = new FolderResolver(
            package.Query("SELECT `Directory`, `Directory_Parent`, `DefaultDir` FROM `Directory`", 3),
            package.Query("SELECT `Component`, `Directory_` FROM `Component`", 2));

        foreach (string[] row in package.Query("SELECT `Component_`, `FileName`, `FileSize` FROM `File`", 3))
        {
            string name = row[1].Substring(row[1].IndexOf('|') + 1);   // keep the long name
            string size = long.TryParse(row[2], out long bytes) ? TextFormat.Size(bytes) : "";

            yield return new FileEntry(name, folders.ForComponent(row[0]), size);
        }
    }
}
