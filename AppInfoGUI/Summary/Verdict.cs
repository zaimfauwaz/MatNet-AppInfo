namespace AppInfoGUI.Summary;

public enum VerdictLevel
{
    Checking,   // grey: still working
    Good,       // green: valid signature and nothing to review
    Review,     // amber: something to look at
    Danger      // red: a real problem
}

public record Verdict(VerdictLevel Level, string Headline, List<Finding> Findings);
