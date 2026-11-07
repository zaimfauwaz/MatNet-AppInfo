using AppInfoGUI.Crypto;
using AppInfoGUI.Driver;

namespace AppInfoGUI.Summary;

// All the rules that turn the findings into one verdict live here, and only here.
// The worst finding wins. Green needs a valid signature (from Windows) and nothing else to report.
public static class SummaryBuilder
{
    public static Verdict Build(SummaryInputs input)
    {
        List<Finding> all = Collect(input);

        // The list shown under the headline: most serious first, neutral facts left out
        List<Finding> notable = all.Where(f => f.Level != FindingLevel.Info)
                                   .OrderBy(f => f.Level).ToList();

        int dangers = notable.Count(f => f.Level == FindingLevel.Danger);
        if (dangers > 0)
        {
            string text = dangers == 1 ? notable[0].Text : $"{dangers} serious problems found";
            return new Verdict(VerdictLevel.Danger, text, notable);
        }

        if (!input.Finished)
            return new Verdict(VerdictLevel.Checking, "Checking...", notable);

        if (notable.Count > 0)
            return new Verdict(VerdictLevel.Review, ReviewHeadline(input, notable.Count), notable);

        if (input.Signature?.IsValid == true)
            return new Verdict(VerdictLevel.Good, "Signature valid, nothing unusual found", notable);

        return new Verdict(VerdictLevel.Review, "Could not decide", notable);
    }

    static string ReviewHeadline(SummaryInputs input, int count)
    {
        string things = count == 1 ? "1 thing to review" : $"{count} things to review";
        return input.Signature?.IsValid == true ? $"Signed by {input.Signature.Publisher} - {things}" : things;
    }

    static List<Finding> Collect(SummaryInputs input)
    {
        var findings = new List<Finding>();

        AddSignature(findings, input);
        AddDownload(findings, input);
        AddRights(findings, input);
        AddPlace(findings, input);
        AddInstaller(findings, input);
        AddDriver(findings, input);

        foreach (string name in input.Failures)
            findings.Add(new(FindingLevel.Unknown, $"Could not check: {name}"));

        return findings;
    }

    static void AddSignature(List<Finding> findings, SummaryInputs input)
    {
        SignatureResult? s = input.Signature;

        if (s == null)
        {
            if (input.Finished) findings.Add(new(FindingLevel.Unknown, "Could not check the signature"));
        }
        else if (s.IsValid)
            findings.Add(new(FindingLevel.Info, $"Signed by {s.Publisher}"));
        else if (s.SignatureType == "None")
            findings.Add(new(FindingLevel.Review, "Not digitally signed"));
        else
            findings.Add(new(FindingLevel.Danger, $"Signature problem: {s.Verdict}"));
    }

    // A download is only worth a flag when nothing vouches for it
    static void AddDownload(List<Finding> findings, SummaryInputs input)
    {
        if (input.Downloaded != true || input.Signature == null) return;

        findings.Add(input.Signature.IsValid
            ? new(FindingLevel.Info, "Downloaded from the internet")
            : new(FindingLevel.Review, "Downloaded from the internet"));
    }

    static void AddRights(List<Finding> findings, SummaryInputs input)
    {
        if (input.RequestedLevel == "requireAdministrator")
            findings.Add(new(FindingLevel.Review, "Asks for administrator rights"));
        else if (input.RequestedLevel == "highestAvailable")
            findings.Add(new(FindingLevel.Review, "Asks for the highest rights available (administrator if you are one)"));

        if (input.AutostartHits > 0)
            findings.Add(new(FindingLevel.Review, "Set to start by itself or run as a service"));
    }

    static void AddPlace(List<Finding> findings, SummaryInputs input)
    {
        if (input.InTempFolder)
            findings.Add(new(FindingLevel.Review, "Located in a temporary folder"));
    }

    static void AddInstaller(List<Finding> findings, SummaryInputs input)
    {
        if (input.SystemActions > 0)
        {
            string what = input.SystemActions == 1 ? "1 program or script" : $"{input.SystemActions} programs or scripts";
            findings.Add(new(FindingLevel.Review, $"Installer runs {what} as the system"));
        }
    }

    static void AddDriver(List<Finding> findings, SummaryInputs input)
    {
        if (input.DriverCheck == DriverCheckOutcome.Match)
            findings.Add(new(FindingLevel.Danger, "Known vulnerable driver"));
        else if (input.DriverCheck == DriverCheckOutcome.NotChecked)
            findings.Add(new(FindingLevel.Unknown, "Could not check against the vulnerable-driver list"));
    }
}
