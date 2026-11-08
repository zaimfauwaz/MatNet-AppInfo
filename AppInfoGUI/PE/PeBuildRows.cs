using AppInfoGUI.Info;
using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;

namespace AppInfoGUI.PE;

// What the file says about how it was built: the debug (PDB) file it points to and whether the build is reproducible.
// Information only. It adds no warning to the summary.
internal static class PeBuildRows
{
    public static void Fill(List<DetailRow> rows, PEReader pe)
    {
        var entries = pe.ReadDebugDirectory();

        if (entries.Any(e => e.Type == DebugDirectoryEntryType.Reproducible))
            rows.Add(new("Reproducible build", "Yes - the build date in the header is a hash, not a real time"));

        // Only the first CodeView entry is described
        DebugDirectoryEntry? codeView = entries.Cast<DebugDirectoryEntry?>().FirstOrDefault(e => e!.Value.Type == DebugDirectoryEntryType.CodeView);
        if (codeView != null)
        {
            CodeViewDebugDirectoryData data = pe.ReadCodeViewDebugDirectoryData(codeView.Value);

            rows.Add(new("Debug file path", data.Path));
            rows.Add(new("What the path shows", Safe.Text(() => WhatPathShows(data.Path))));
            rows.Add(new("Debug file ID", $"{data.Guid:D}, age {data.Age}"));
            return;
        }

        if (rows.Count == 0)
            rows.Add(new("Debug information", "None in the file (normal for a release build)"));
    }

    // A path like C:\Users\jdoe\source\... names the developer's account
    static string WhatPathShows(string path)
    {
        Match user = Regex.Match(path, @"^[A-Za-z]:\\Users\\([^\\]+)\\", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
        if (user.Success)
            return $"The developer's account name (\"{user.Groups[1].Value}\") and project folder";

        return path.StartsWith(@"\\") ? "A network location" : "A build folder (no personal name in it)";
    }
}
