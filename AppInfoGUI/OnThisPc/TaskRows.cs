using AppInfoGUI.Info;
using System.IO;
using System.Xml.Linq;

namespace AppInfoGUI.OnThisPc;

// Scheduled tasks are stored by Windows as small XML files
internal static class TaskRows
{
    public static void Fill(List<DetailRow> rows, string path)
    {
        string folder = Path.Combine(Environment.SystemDirectory, "Tasks");
        if (!Directory.Exists(folder)) return;

        // Tasks that Windows keeps private are skipped, not treated as errors
        var options = new EnumerationOptions { RecurseSubdirectories = true, IgnoreInaccessible = true };

        foreach (string file in Directory.EnumerateFiles(folder, "*", options))
        {
            try
            {
                if (RunsFile(file, path))
                    rows.Add(new("Scheduled task: " + Path.GetRelativePath(folder, file), "Runs this file"));
            }
            catch (Exception)
            {
                // One unreadable task must not stop the search
            }
        }
    }

    static bool RunsFile(string taskFile, string path)
    {
        XElement root = XDocument.Load(taskFile).Root!;

        return root.Descendants().Any(e => e.Name.LocalName is "Command" or "Arguments"
                                           && PathMatch.Mentions(e.Value, path));
    }
}
