using System.ComponentModel;
using System.Diagnostics;
using System.Security.Principal;

namespace AppInfoGUI.Startup;

// Running as administrator is opt-in: the "(administrator)" right-click entry adds --admin.
// Without it the app runs with normal rights.
internal static class Elevation
{
    public const string Flag = "--admin";

    public static bool WantsAdmin(string[] args) =>
        args.Any(a => a.Equals(Flag, StringComparison.OrdinalIgnoreCase));

    public static bool IsElevated
    {
        get
        {
            using WindowsIdentity identity = WindowsIdentity.GetCurrent();
            return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
        }
    }

    // Starts a second copy of the app with the Windows permission prompt (UAC).
    // False means it did not start, for example because the prompt was cancelled.
    public static bool TryRelaunch(string[] args)
    {
        string? exe = Environment.ProcessPath;
        if (exe == null) return false;

        var start = new ProcessStartInfo
        {
            FileName = exe,
            Arguments = string.Join(" ", args.Select(Quote)),
            UseShellExecute = true,
            Verb = "runas"
        };

        try
        {
            Process.Start(start);
            return true;
        }
        catch (Win32Exception)
        {
            return false;   // the user said no to the prompt
        }
    }

    static string Quote(string argument) => "\"" + argument.Replace("\"", "\\\"") + "\"";
}
