namespace AppInfoGUI.Info;

internal static class TextFormat
{
    public static string Size(long bytes)
    {
        if (bytes < 1024) return $"{bytes} bytes";

        string readable = bytes < 1024 * 1024
            ? $"{bytes / 1024.0:0.0} KB"
            : $"{bytes / 1024.0 / 1024.0:0.0} MB";

        return $"{readable} ({bytes:N0} bytes)";
    }

    // A file that claims a different original name than the one it has now is worth noticing
    public static string OriginalName(string? original, string currentName)
    {
        if (string.IsNullOrWhiteSpace(original)) return "";

        bool same = string.Equals(original, currentName, StringComparison.OrdinalIgnoreCase);
        return same ? original : $"{original}  (differs from current file name)";
    }
}
