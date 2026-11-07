using AppInfoGUI.Info;
using System.Xml.Linq;

namespace AppInfoGUI.PE;

// Turns the manifest text into readable rows
internal static class ManifestRows
{
    public static void Fill(List<DetailRow> rows, string path)
    {
        string? text;

        try
        {
            text = ManifestText.Read(path);
        }
        catch (Exception ex)
        {
            rows.Add(new("Manifest", "Could not read: " + ex.Message));
            return;
        }

        if (text == null)
        {
            rows.Add(new("Manifest", "No embedded manifest"));
            return;
        }

        XElement root;
        try
        {
            root = XDocument.Parse(text).Root!;
        }
        catch (Exception)
        {
            rows.Add(new("Manifest", "Found, but its text is not valid XML"));
            return;
        }

        // Each row is read on its own, so one odd entry never hides the others
        rows.Add(new("Runs as", Safe.Text(() => ExecutionLevel(root))));
        rows.Add(new("Identity", Safe.Text(() => Identity(root))));
        rows.Add(new("Screen scaling (DPI)", Safe.Text(() => Setting(root, "dpiAwareness") ?? Setting(root, "dpiAware") ?? "Not stated")));
        rows.Add(new("Long paths", Safe.Text(() => Setting(root, "longPathAware") ?? "Not stated")));
        rows.Add(new("Supported Windows", Safe.Text(() => SupportedWindows(root))));
        rows.Add(new("Visual styles", Safe.Text(() => VisualStyles(root))));
    }

    // The "level" the manifest asks for, for example "requireAdministrator". Null when it does not say.
    public static string? RequestedLevel(string path)
    {
        string? text = ManifestText.Read(path);
        if (text == null) return null;

        XElement root = XDocument.Parse(text).Root!;
        return Attribute(Find(root, "requestedExecutionLevel").FirstOrDefault(), "level");
    }

    // Elements are found by their local name, because manifests use many namespaces
    static IEnumerable<XElement> Find(XElement root, string name) =>
        root.Descendants().Where(e => e.Name.LocalName == name);

    static string? Attribute(XElement? element, string name) => element?.Attribute(name)?.Value;

    static string ExecutionLevel(XElement root)
    {
        XElement? level = Find(root, "requestedExecutionLevel").FirstOrDefault();
        if (level == null) return "Not stated (Windows decides)";

        string text = Attribute(level, "level") switch
        {
            "asInvoker" => "The user who starts it (normal)",
            "highestAvailable" => "The highest rights the user has (asks for administrator if the user is one)",
            "requireAdministrator" => "Administrator (always asks for permission)",
            string other => other,
            null => "Not stated"
        };

        // UI access lets a program control other programs' windows
        if (Attribute(level, "uiAccess") == "true") text += ". Can control other programs' windows";
        return text;
    }

    static string Identity(XElement root)
    {
        XElement? identity = Find(root, "assemblyIdentity").FirstOrDefault();
        string name = Attribute(identity, "name") ?? "";
        string version = Attribute(identity, "version") ?? "";
        string text = $"{name} {version}".Trim();
        return text == "" ? "Not stated" : text;
    }

    static string? Setting(XElement root, string name) =>
        Find(root, name).FirstOrDefault()?.Value.Trim() is { Length: > 0 } value ? value : null;

    static string SupportedWindows(XElement root)
    {
        var names = new List<string>();

        foreach (XElement os in Find(root, "supportedOS"))
        {
            string id = (Attribute(os, "Id") ?? "").Trim('{', '}').ToLowerInvariant();
            names.Add(WindowsName(id));
        }

        return names.Count == 0 ? "Not stated" : string.Join(", ", names);
    }

    // The fixed IDs Microsoft publishes for each Windows version
    static string WindowsName(string id) => id switch
    {
        "e2011457-1546-43c5-a5fe-008deee3d3f0" => "Vista",
        "35138b9a-5d96-4fbd-8e2d-a2440225f93a" => "7",
        "4a2f28e3-53b9-4441-ba9c-d69d4a4a6e38" => "8",
        "1f676c76-80e1-4239-95bb-83d0f6d0da78" => "8.1",
        "8e0f7a12-bfb3-4fe8-b9a5-48fd50a15a9a" => "10 and 11",
        _ => "Unknown (" + id + ")"
    };

    static string VisualStyles(XElement root)
    {
        bool modern = Find(root, "assemblyIdentity")
            .Any(e => Attribute(e, "name") == "Microsoft.Windows.Common-Controls");

        return modern ? "Modern (Common Controls 6)" : "Classic";
    }
}
