namespace AppInfoGUI.Installer;

// Turns the package's folder tables into readable paths like "Program Files\My Company\App"
internal class FolderResolver
{
    // The folders Windows Installer already knows by name
    static readonly Dictionary<string, string> StandardFolders = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ProgramFilesFolder"] = @"Program Files (x86)",
        ["ProgramFiles64Folder"] = @"Program Files",
        ["ProgramFiles6432Folder"] = @"Program Files",
        ["CommonFilesFolder"] = @"Common Files (x86)",
        ["CommonFiles64Folder"] = @"Common Files",
        ["CommonFiles6432Folder"] = @"Common Files",
        ["SystemFolder"] = @"Windows\SysWOW64",
        ["System64Folder"] = @"Windows\System32",
        ["WindowsFolder"] = @"Windows",
        ["FontsFolder"] = @"Windows\Fonts",
        ["AppDataFolder"] = @"%APPDATA%",
        ["LocalAppDataFolder"] = @"%LOCALAPPDATA%",
        ["CommonAppDataFolder"] = @"%PROGRAMDATA%",
        ["TempFolder"] = @"%TEMP%",
        ["DesktopFolder"] = @"Desktop",
        ["StartMenuFolder"] = @"Start Menu",
        ["ProgramMenuFolder"] = @"Start Menu\Programs",
        ["StartupFolder"] = @"Startup",
        ["PersonalFolder"] = @"Documents"
    };

    readonly Dictionary<string, (string Parent, string Name)> _folders = new(StringComparer.OrdinalIgnoreCase);
    readonly Dictionary<string, string> _componentFolder = new(StringComparer.OrdinalIgnoreCase);

    // directories: Directory, Directory_Parent, DefaultDir.  components: Component, Directory_.
    public FolderResolver(IEnumerable<string[]> directories, IEnumerable<string[]> components)
    {
        foreach (string[] row in directories)
            _folders[row[0]] = (row[1], row[2]);

        foreach (string[] row in components)
            _componentFolder[row[0]] = row[1];
    }

    public string ForComponent(string component) =>
        _componentFolder.TryGetValue(component, out string? folder) ? PathOf(folder) : "Unknown";

    string PathOf(string folder)
    {
        var parts = new List<string>();
        string current = folder;

        // The limit stops a broken package from looping forever
        for (int i = 0; i < 20 && current != ""; i++)
        {
            if (StandardFolders.TryGetValue(current, out string? standard))
            {
                parts.Add(standard);
                break;
            }

            if (!_folders.TryGetValue(current, out var entry))
            {
                parts.Add($"[{current}]");
                break;
            }

            string name = ReadableName(entry.Name);
            if (name != "") parts.Add(name);
            current = entry.Parent;
        }

        parts.Reverse();
        return string.Join("\\", parts);
    }

    // A folder name can look like "SHORT~1|Long name" or "target:source". We want the long target name.
    static string ReadableName(string defaultDir)
    {
        string name = defaultDir.Split(':')[0];
        name = name.Substring(name.IndexOf('|') + 1);
        return name == "." ? "" : name;
    }
}
