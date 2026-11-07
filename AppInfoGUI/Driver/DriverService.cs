using AppInfoGUI.Info;
using System.IO;
using Microsoft.Win32;

namespace AppInfoGUI.Driver;

public record DriverService(string Name, string DisplayName, string StartType, string DriverType);

// Looks in Windows' own list of drivers (read only) for one that points to this file
internal static class DriverServiceFinder
{
    public static DriverService? Find(string path)
    {
        using RegistryKey? services = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services");
        if (services == null) return null;

        string target = Path.GetFullPath(path);

        foreach (string name in services.GetSubKeyNames())
        {
            try
            {
                using RegistryKey? key = services.OpenSubKey(name);
                if (key?.GetValue("Type") is not int type || (type & 0x0F) == 0)
                    continue;   // not a driver

                // Without an ImagePath, Windows looks in the standard driver folder
                string imagePath = key.GetValue("ImagePath") as string ?? $@"\SystemRoot\System32\drivers\{name}.sys";

                if (FullPath(imagePath).Equals(target, StringComparison.OrdinalIgnoreCase))
                    return Describe(name, key, type);
            }
            catch (Exception)
            {
                // One unreadable entry must not stop the search
            }
        }

        return null;
    }

    static DriverService Describe(string name, RegistryKey key, int type)
    {
        string display = key.GetValue("DisplayName") as string ?? "";
        if (display == "" || display.StartsWith('@')) display = name;

        string start = key.GetValue("Start") is int s ? ServiceStartType.Describe(s) : "Not available";
        return new DriverService(name, display, start, TypeName(type));
    }

    // Drivers write their path in several styles. This turns them all into a normal path.
    static string FullPath(string imagePath)
    {
        string path = imagePath.Trim().Trim('"');
        string windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);

        if (path.StartsWith(@"\??\"))
            path = path.Substring(4);
        else if (path.StartsWith(@"\SystemRoot\", StringComparison.OrdinalIgnoreCase))
            path = Path.Combine(windows, path.Substring(12));
        else if (!Path.IsPathRooted(path))
            path = Path.Combine(windows, path);

        return Path.GetFullPath(path);
    }

    static string TypeName(int type) => (type & 0x0F) switch
    {
        1 => "Kernel driver",
        2 => "File system driver",
        4 => "Adapter",
        8 => "Recognizer driver",
        _ => $"Other ({type})"
    };
}
