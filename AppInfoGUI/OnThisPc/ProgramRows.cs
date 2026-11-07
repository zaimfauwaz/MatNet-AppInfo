using AppInfoGUI.Info;
using Microsoft.Win32;

namespace AppInfoGUI.OnThisPc;

// The list behind "Programs & Features"
internal static class ProgramRows
{
    const string Uninstall = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
    const string Uninstall32 = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall";

    public static List<DetailRow> Read(string path)
    {
        var rows = new List<DetailRow>();

        Search(rows, Registry.LocalMachine, Uninstall, path);
        Search(rows, Registry.LocalMachine, Uninstall32, path);
        Search(rows, Registry.CurrentUser, Uninstall, path);

        return rows;
    }

    static void Search(List<DetailRow> rows, RegistryKey root, string subKey, string path)
    {
        using RegistryKey? list = root.OpenSubKey(subKey);
        if (list == null) return;

        foreach (string name in list.GetSubKeyNames())
        {
            try
            {
                using RegistryKey? entry = list.OpenSubKey(name);
                if (entry == null || !Belongs(entry, path)) continue;

                string title = entry.GetValue("DisplayName") as string ?? name;
                string version = entry.GetValue("DisplayVersion") as string ?? "";
                string publisher = entry.GetValue("Publisher") as string ?? "";

                rows.Add(new(title, string.Join(" - ", new[] { version, publisher }.Where(s => s != ""))));
            }
            catch (Exception)
            {
                // One unreadable entry must not stop the search
            }
        }
    }

    // The file is part of the program if it sits in the install folder or the entry points to it
    static bool Belongs(RegistryKey entry, string path) =>
        PathMatch.IsInside(entry.GetValue("InstallLocation") as string, path)
        || PathMatch.Mentions(entry.GetValue("DisplayIcon") as string, path)
        || PathMatch.Mentions(entry.GetValue("UninstallString") as string, path);
}
