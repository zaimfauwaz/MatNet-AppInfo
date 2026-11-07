namespace AppInfoGUI.Installer;

// Explains what a custom action does, from its type number
internal static class CustomActionKind
{
    const int Deferred = 0x400;
    const int NoImpersonate = 0x800;

    public static string Describe(int type) => (type & 0x3F) switch
    {
        1 or 17 => "Runs code from a DLL",
        2 or 18 or 34 or 50 => "Runs a program",
        5 or 21 or 37 or 53 => "Runs a script (JScript)",
        6 or 22 or 38 or 54 => "Runs a script (VBScript)",
        35 or 51 => "Sets a value (harmless)",
        19 => "Shows an error message",
        _ => $"Other (type {type & 0x3F})"
    };

    // Deferred actions that do not impersonate the user run as the system
    public static string RunsAs(int type)
    {
        if ((type & (Deferred | NoImpersonate)) == (Deferred | NoImpersonate)) return "System (elevated)";
        if ((type & Deferred) != 0) return "Installing user";
        return "Immediately";
    }

    public static bool IsScriptText(int type) => (type & 0x3F) is 37 or 38;
    public static bool IsHarmless(int type) => (type & 0x3F) is 35 or 51 or 19;
}
