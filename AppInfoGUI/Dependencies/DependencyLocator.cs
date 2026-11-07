using System.IO;

namespace AppInfoGUI.Dependencies;

// Says where Windows would most likely find an imported DLL
internal static class DependencyLocator
{
    public static string Where(string dll, string fileFolder, bool is32Bit)
    {
        // These names are not real files. Windows maps them to the real DLLs itself.
        if (dll.StartsWith("api-ms-", StringComparison.OrdinalIgnoreCase)
            || dll.StartsWith("ext-ms-", StringComparison.OrdinalIgnoreCase))
            return "Windows API set (virtual)";

        if (File.Exists(Path.Combine(fileFolder, dll)))
            return "Same folder as this file";

        string system = is32Bit
            ? Environment.GetFolderPath(Environment.SpecialFolder.SystemX86)
            : Environment.SystemDirectory;

        if (File.Exists(Path.Combine(system, dll)))
            return "Windows system folder";

        string windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        if (File.Exists(Path.Combine(windows, dll)))
            return "Windows folder";

        return "Not found here (may be in PATH or loaded later)";
    }
}
