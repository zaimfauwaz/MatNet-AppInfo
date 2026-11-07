namespace AppInfoGUI.OnThisPc;

internal static class PathMatch
{
    // True when a setting from Windows (a command line, a registry value) mentions the file.
    // Windows settings often use quotes and variables like %ProgramFiles%.
    public static bool Mentions(string? setting, string path)
    {
        if (string.IsNullOrWhiteSpace(setting)) return false;

        string text = Environment.ExpandEnvironmentVariables(setting).Replace("\"", "");
        return text.Contains(path, StringComparison.OrdinalIgnoreCase);
    }

    // True when the file lives inside a folder (for example an install location)
    public static bool IsInside(string? folder, string path)
    {
        if (string.IsNullOrWhiteSpace(folder)) return false;

        string clean = Environment.ExpandEnvironmentVariables(folder).Trim('"', ' ').TrimEnd('\\');
        return clean.Length > 3 && path.StartsWith(clean + "\\", StringComparison.OrdinalIgnoreCase);
    }
}
