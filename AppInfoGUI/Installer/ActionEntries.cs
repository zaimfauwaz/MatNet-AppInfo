namespace AppInfoGUI.Installer;

internal static class ActionEntries
{
    public static IEnumerable<ActionEntry> Read(MsiPackage package)
    {
        var actions = new List<(ActionEntry Entry, bool Harmless)>();

        foreach (string[] row in package.Query("SELECT `Action`, `Type`, `Source`, `Target` FROM `CustomAction`", 4))
        {
            if (!int.TryParse(row[1], out int type)) continue;

            actions.Add((new ActionEntry(row[0], CustomActionKind.Describe(type), CustomActionKind.RunsAs(type), Detail(type, row[2], row[3])),
                         CustomActionKind.IsHarmless(type)));
        }

        // The ones that run something come first
        return actions.OrderBy(a => a.Harmless).Select(a => a.Entry);
    }

    // The text of a script is long and not useful in a table, so only its size is shown
    static string Detail(int type, string source, string target)
    {
        if (CustomActionKind.IsScriptText(type))
            return $"(script text, {target.Length} characters)";

        string text = target != "" ? target : source;
        return text.Length > 150 ? text.Substring(0, 150) + "..." : text;
    }
}
