using System.Text.RegularExpressions;

namespace AppInfoGUI.Strings;

// Picks the interesting pieces out of a line of text
internal static partial class StringClassifier
{
    public static IEnumerable<(string Kind, string Text, int Index)> Find(string text)
    {
        if (text.Contains("://"))
            foreach ((string, string, int) hit in Matches(StringKinds.Web, WebPattern(), text))
                yield return hit;

        if (text.Contains('@'))
            foreach ((string, string, int) hit in Matches(StringKinds.Email, EmailPattern(), text))
                yield return hit;

        if (text.Contains('.'))
            foreach ((string, string, int) hit in Matches(StringKinds.Ip, IpPattern(), text, m => !LooksLikeVersion(m.Value)))
                yield return hit;

        if (!text.Contains('\\'))
            yield break;

        foreach ((string, string, int) hit in Matches(StringKinds.Registry, RegistryPattern(), text, trim: true))
            yield return hit;

        foreach ((string, string, int) hit in Matches(StringKinds.Path, PathPattern(), text, trim: true))
            yield return hit;
    }

    static IEnumerable<(string Kind, string Text, int Index)> Matches(
        string kind, Regex pattern, string text, Func<Match, bool>? keep = null, bool trim = false)
    {
        foreach (Match m in pattern.Matches(text).Where(m => keep == null || keep(m)))
            yield return (kind, trim ? m.Value.TrimEnd() : m.Value, m.Index);
    }

    // Version numbers and certificate identifiers (OIDs) look like addresses. These are the patterns we can tell apart safely:
    // "0.x", "x.x.x.0" (1.0.0.0), the identifier families 1.2 / 1.3 / 2.5 (2.5.4.3 is "common name") / 2.16 / 2.23,
    // and a 0 in the middle of a public address (6.4.0.1).
    // Home and office ranges are kept, since 10.0.0.1 and 192.168.0.1 are real.
    static readonly string[] IdentifierStarts = { "1.2.", "1.3.", "2.5.", "2.16.", "2.23." };

    static bool LooksLikeVersion(string address)
    {
        if (address.StartsWith("0.") || address.EndsWith(".0") || IdentifierStarts.Any(address.StartsWith))
            return true;

        bool privateRange = address.StartsWith("10.") || address.StartsWith("192.168.") || address.StartsWith("127.");
        return !privateRange && address.Contains(".0.");
    }

    [GeneratedRegex(@"https?://[^\s""'<>\\^`{|}]{4,}", RegexOptions.IgnoreCase, 1000)]
    private static partial Regex WebPattern();

    [GeneratedRegex(@"\b[A-Za-z0-9._%+\-]{3,64}@[A-Za-z0-9\-]{3,63}(?:\.[A-Za-z0-9\-]{1,63})*\.(?:[a-z]{2,24}|[A-Z]{2,24})\b", RegexOptions.None, 1000)]
    private static partial Regex EmailPattern();

    [GeneratedRegex(@"(?<![\d.])(?:(?:25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)\.){3}(?:25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(?![\d.])", RegexOptions.None, 1000)]
    private static partial Regex IpPattern();

    [GeneratedRegex(@"(?:\bHKEY_[A-Z_]+|\bHK(?:LM|CU|CR)|\bSOFTWARE)\\[\w .\-{}\\]{2,}", RegexOptions.None, 1000)]
    private static partial Regex RegistryPattern();

    [GeneratedRegex(@"(?:\b[A-Za-z]:|%[A-Za-z_]+%)\\[\w .\-$()\\]{6,}", RegexOptions.None, 1000)]
    private static partial Regex PathPattern();
}
