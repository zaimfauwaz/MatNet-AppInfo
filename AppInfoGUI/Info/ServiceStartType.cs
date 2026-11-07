namespace AppInfoGUI.Info;

// Windows describes how a service or driver starts with a number. This turns it into words.
internal static class ServiceStartType
{
    public static string Describe(int start) => start switch
    {
        0 => "Boot (loads first, with Windows)",
        1 => "System (loads with Windows)",
        2 => "Automatic",
        3 => "On demand (only when needed)",
        4 => "Disabled",
        _ => $"Unknown ({start})"
    };

    // Installer packages store the number as text
    public static string Describe(string start) => int.TryParse(start, out int number) ? Describe(number) : start;
}
