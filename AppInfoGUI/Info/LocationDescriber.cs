using System.IO;

namespace AppInfoGUI.Info;

internal static class LocationDescriber
{
    public static string Describe(string folder)
    {
        string profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        // The first match wins, so specific places come before general ones
        var places = new (string Folder, string Label)[]
        {
            (Path.GetTempPath(), "Temporary folder"),
            (Path.Combine(profile, "Downloads"), "Downloads folder"),
            (Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Desktop"),
            (Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "User application data (AppData)"),
            (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "User application data (AppData)"),
            (profile, "User profile folder"),
            (Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Windows folder"),
            (Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Program Files"),
            (Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Program Files (x86)")
        };

        foreach (var place in places)
        {
            if (IsInside(folder, place.Folder))
                return place.Label;
        }

        return "Other location";
    }

    // True if the folder is the parent itself or anywhere below it
    internal static bool IsInside(string folder, string parent)
    {
        parent = parent.TrimEnd('\\');
        if (parent == "") return false;

        return folder.Equals(parent, StringComparison.OrdinalIgnoreCase)
            || folder.StartsWith(parent + "\\", StringComparison.OrdinalIgnoreCase);
    }
}
