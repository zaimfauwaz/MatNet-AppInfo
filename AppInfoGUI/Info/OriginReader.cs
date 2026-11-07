using System.IO;

namespace AppInfoGUI.Info;

internal static class OriginReader
{
    // Windows marks downloaded files with a hidden stream called Zone.Identifier.
    // It looks like this:  ZoneId=3  /  ReferrerUrl=https://...  /  HostUrl=https://...
    public static string MarkOfTheWeb(string path)
    {
        string[]? lines = ReadMark(path);
        if (lines == null)
            return "None (not marked as downloaded)";

        string? zone = Value(lines, "ZoneId");
        return zone == null ? "Marked, but the zone is unknown" : ZoneName(zone);
    }

    // The web page the download was started from
    public static string? DownloadedFrom(string path) => Value(ReadMark(path), "ReferrerUrl") ?? NotRecorded(path);

    // The address of the file itself
    public static string? DownloadLink(string path) => Value(ReadMark(path), "HostUrl") ?? NotRecorded(path);

    // Null means the file was never marked
    static string[]? ReadMark(string path)
    {
        try
        {
            return File.ReadAllLines(path + ":Zone.Identifier");
        }
        catch (FileNotFoundException)
        {
            return null;
        }
    }

    static string? Value(string[]? lines, string name)
    {
        if (lines == null) return null;

        foreach (string line in lines)
        {
            if (line.StartsWith(name + "=", StringComparison.OrdinalIgnoreCase))
            {
                string value = line.Substring(name.Length + 1).Trim();
                return value == "" ? null : value;
            }
        }

        return null;
    }

    static string NotRecorded(string path) =>
        ReadMark(path) == null ? "Not recorded (file is not marked as downloaded)" : "Not recorded";

    static string ZoneName(string id) => id switch
    {
        "0" => "Local computer",
        "1" => "Local intranet",
        "2" => "Trusted sites",
        "3" => "Internet (downloaded)",
        "4" => "Restricted sites",
        _ => $"Unknown zone ({id})"
    };
}
