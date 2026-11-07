using AppInfoGUI.Info;
using Microsoft.Win32;

namespace AppInfoGUI.OnThisPc;

// Places where Windows is told to start something by itself
internal static class AutostartRows
{
    static readonly string[] RunKeys =
    {
        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run",
        @"SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce",
        @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Run"
    };

    public static List<DetailRow> Read(string path)
    {
        var rows = new List<DetailRow>();

        // Each place is checked on its own
        Try(rows, "Run keys", () => FromRunKeys(rows, path));
        Try(rows, "Services", () => FromServices(rows, path));
        Try(rows, "Scheduled tasks", () => TaskRows.Fill(rows, path));

        return rows;
    }

    static void Try(List<DetailRow> rows, string place, Action check)
    {
        try
        {
            check();
        }
        catch (Exception ex)
        {
            rows.Add(new(place, "Could not check: " + ex.Message));
        }
    }

    static void FromRunKeys(List<DetailRow> rows, string path)
    {
        foreach (RegistryKey root in new[] { Registry.LocalMachine, Registry.CurrentUser })
        {
            string owner = root == Registry.LocalMachine ? "all users" : "current user";

            foreach (string subKey in RunKeys)
            {
                using RegistryKey? key = root.OpenSubKey(subKey);
                if (key == null) continue;

                foreach (string name in key.GetValueNames())
                {
                    string? command = key.GetValue(name) as string;
                    if (PathMatch.Mentions(command, path))
                        rows.Add(new($"Run key ({owner}): {name}", command!));
                }
            }
        }
    }

    static void FromServices(List<DetailRow> rows, string path)
    {
        using RegistryKey? services = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services");
        if (services == null) return;

        foreach (string name in services.GetSubKeyNames())
        {
            try
            {
                using RegistryKey? service = services.OpenSubKey(name);
                if (!PathMatch.Mentions(service?.GetValue("ImagePath") as string, path)) continue;

                string start = service!.GetValue("Start") is int s ? ServiceStartType.Describe(s) : "start type not available";
                rows.Add(new($"Service: {name}", start));
            }
            catch (Exception)
            {
                // One unreadable service must not stop the search
            }
        }
    }
}
