namespace AppInfoGUI.Summary;

// How much a finding matters. The order matters: the most serious comes first.
public enum FindingLevel
{
    Danger,    // a real problem
    Review,    // worth a look, not proof of a problem
    Unknown,   // we could not check. This is never treated as fine.
    Info       // a neutral fact
}

public record Finding(FindingLevel Level, string Text);
