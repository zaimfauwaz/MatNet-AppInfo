using System.IO;
using Microsoft.Win32;

namespace AppInfoGUI.Driver;

// Finds out whether Windows has this file registered as an ActiveX/COM control
internal static class ComRegistration
{
    static readonly (RegistryKey Root, string Path)[] Places =
    {
        (Registry.LocalMachine, @"SOFTWARE\Classes\CLSID"),
        (Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Classes\CLSID"),   // 32-bit controls
        (Registry.CurrentUser, @"Software\Classes\CLSID")                 // registered for this user only
    };

    // Returns the control's ID and name, or null if it is not registered
    public static string? Find(string path)
    {
        string target = Path.GetFullPath(path);

        foreach (var place in Places)
        {
            using RegistryKey? clsids = place.Root.OpenSubKey(place.Path);
            if (clsids == null) continue;

            foreach (string id in clsids.GetSubKeyNames())
            {
                try
                {
                    using RegistryKey? server = clsids.OpenSubKey(id + @"\InprocServer32");
                    string registered = (server?.GetValue(null) as string ?? "").Trim().Trim('"');
                    if (registered == "") continue;

                    if (Path.GetFullPath(registered).Equals(target, StringComparison.OrdinalIgnoreCase))
                        return id;
                }
                catch (Exception)
                {
                    // One unreadable entry must not stop the search
                }
            }
        }

        return null;
    }
}
