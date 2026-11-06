using System.IO;

namespace AppInfoGUI.Info;

internal static class OriginReader
{
    // Windows marks downloaded files with a hidden stream called Zone.Identifier
    public static string MarkOfTheWeb(string path)
    {
        string[] lines;
        try
        {
            lines = File.ReadAllLines(path + ":Zone.Identifier");
        }
        catch (FileNotFoundException)
        {
            return "None (not marked as downloaded)";
        }

        foreach (string line in lines)
        {
            if (line.StartsWith("ZoneId=", StringComparison.OrdinalIgnoreCase))
                return ZoneName(line.Substring("ZoneId=".Length).Trim());
        }

        return "Marked, but the zone is unknown";
    }

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
