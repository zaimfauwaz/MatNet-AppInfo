namespace AppInfoGUI.Installer;

internal static class RegistryEntries
{
    public static IEnumerable<RegistryEntry> Read(MsiPackage package)
    {
        foreach (string[] row in package.Query("SELECT `Root`, `Key`, `Name`, `Value` FROM `Registry`", 4))
            yield return new RegistryEntry(RootName(row[0]), row[1], NameText(row[2]), Shorten(row[3]));
    }

    static string RootName(string root) => root switch
    {
        "0" => "HKEY_CLASSES_ROOT",
        "1" => "HKEY_CURRENT_USER",
        "2" => "HKEY_LOCAL_MACHINE",
        "3" => "HKEY_USERS",
        "-1" => "Current user or all users (chosen at install)",
        _ => root
    };

    // Some names are instructions, not names
    static string NameText(string name) => name switch
    {
        "" => "(Default)",
        "+" => "(create the key)",
        "-" => "(delete the key)",
        "*" => "(delete the key and everything below)",
        _ => name
    };

    static string Shorten(string value) => value.Length > 200 ? value.Substring(0, 200) + "..." : value;
}
