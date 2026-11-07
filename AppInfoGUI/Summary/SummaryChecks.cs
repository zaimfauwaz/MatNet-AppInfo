using AppInfoGUI.Info;
using AppInfoGUI.PE;
using System.IO;

namespace AppInfoGUI.Summary;

// What the summary needs that no tab has already read. Each check runs on its own.
public class FileFacts
{
    public bool? Downloaded { get; set; }
    public string? RequestedLevel { get; set; }
    public bool InTempFolder { get; set; }
    public List<string> Failures { get; } = new();
}

public static class SummaryChecks
{
    public static FileFacts Read(string path, bool isInstaller)
    {
        var facts = new FileFacts();

        Try(facts, "download origin", () => facts.Downloaded = IsDownloaded(path));
        Try(facts, "file location", () => facts.InTempFolder = InTempFolder(path));

        // Installer packages have no manifest
        if (!isInstaller)
            Try(facts, "manifest", () => facts.RequestedLevel = ManifestRows.RequestedLevel(path));

        return facts;
    }

    static void Try(FileFacts facts, string name, Action check)
    {
        try
        {
            check();
        }
        catch (Exception)
        {
            facts.Failures.Add(name);
        }
    }

    // Zone 3 is the internet, zone 4 is a site Windows was told to distrust
    static bool IsDownloaded(string path) => OriginReader.ZoneId(path) is "3" or "4";

    static bool InTempFolder(string path) =>
        LocationDescriber.Describe(Path.GetDirectoryName(path) ?? "") == "Temporary folder";
}
