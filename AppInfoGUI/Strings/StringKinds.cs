namespace AppInfoGUI.Strings;

// The kinds of text worth listing, in the order they are shown
public static class StringKinds
{
    public const string Web = "Web address";
    public const string Ip = "IP address";
    public const string Registry = "Registry key";
    public const string Path = "File path";
    public const string Email = "Email";

    public static readonly string[] All = { Web, Ip, Registry, Path, Email };

    // The label on the filter button
    public static string Plural(string kind) => kind switch
    {
        Web => "Web addresses",
        Ip => "IP addresses",
        Registry => "Registry keys",
        Path => "File paths",
        _ => "Emails"
    };
}
