namespace AppInfoGUI.Installer;

// One row in each of the tables on the "Installer Contents" tab
public record FileEntry(string Name, string Folder, string Size);
public record RegistryEntry(string Root, string Key, string Name, string Value);
public record ServiceEntry(string Name, string DisplayName, string Start, string Account);
public record ActionEntry(string Name, string Kind, string RunsAs, string Detail)
{
    // Runs a program, script or DLL with full system rights
    public bool RunsCodeAsSystem => RunsAs.StartsWith("System") && Kind.StartsWith("Runs");
}

// The rows to show, and how many there really are
public class InstallSection<T>
{
    public List<T> Rows { get; } = new();
    public int Total { get; set; }
}

public class InstallContents
{
    public string Message { get; set; } = "";
    public InstallSection<FileEntry> Files { get; } = new();
    public InstallSection<RegistryEntry> Registry { get; } = new();
    public InstallSection<ServiceEntry> Services { get; } = new();
    public InstallSection<ActionEntry> Actions { get; } = new();
}
