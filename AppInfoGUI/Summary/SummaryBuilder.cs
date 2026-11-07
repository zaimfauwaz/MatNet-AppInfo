using AppInfoGUI.Crypto;
using AppInfoGUI.Driver;
using System.IO;

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
        AddNameMismatch(findings, input);
        AddInstaller(findings, input);
        AddPackage(findings, input);
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
            findings.Add(new(FindingLevel.Review, "This program asks for administrator rights every time it starts"));
        else if (input.RequestedLevel == "highestAvailable")
            findings.Add(new(FindingLevel.Review, "This program asks for the highest rights available when it starts (administrator, if you are one)"));

        if (input.AutostartHits > 0)
            findings.Add(new(FindingLevel.Review, "This program is set to start by itself or run as a service"));
    }

    static void AddPlace(List<Finding> findings, SummaryInputs input)
    {
        if (input.InTempFolder)
            findings.Add(new(FindingLevel.Review, "This file is in a temporary folder"));
    }

    // Only a different file type counts. Renaming "setup.exe" to "app-1.2.exe" is normal.
    static void AddNameMismatch(List<Finding> findings, SummaryInputs input)
    {
        string original = input.OriginalName?.Trim() ?? "";
        string current = input.FileName ?? "";
        string originalType = Path.GetExtension(original);
        string currentType = Path.GetExtension(current);
        if (originalType == "" || currentType == "") return;

        if (!originalType.Equals(currentType, StringComparison.OrdinalIgnoreCase))
            findings.Add(new(FindingLevel.Review, $"The file says it was built as \"{original}\", but it is now named \"{current}\""));
    }

    static void AddInstaller(List<Finding> findings, SummaryInputs input)
    {
        if (input.SystemActions > 0)
        {
            string what = input.SystemActions == 1 ? "1 program or script" : $"{input.SystemActions} programs or scripts";
            findings.Add(new(FindingLevel.Review, $"This installer runs {what} with full system rights"));
        }
    }

    // What was found inside an installer or self-extracting archive
    static void AddPackage(List<Finding> findings, SummaryInputs input)
    {
        if (input.PackageName == null) return;

        if (input.PackageDisguised > 0)
            findings.Add(new(FindingLevel.Review, $"The package holds {input.PackageDisguised} program(s) disguised as documents (for example invoice.pdf.exe)"));

        if (input.PackageScripts > 0)
            findings.Add(new(FindingLevel.Review, $"The package holds {input.PackageScripts} script(s) or shortcut(s)"));

        if (input.PackageEncrypted > 0)
            findings.Add(new(FindingLevel.Review, "Some files in the package are password-protected, so they cannot be inspected"));
    }

    static void AddDriver(List<Finding> findings, SummaryInputs input)
    {
        if (input.DriverCheck == DriverCheckOutcome.Match)
            findings.Add(new(FindingLevel.Danger, "Known vulnerable driver"));
        else if (input.DriverCheck == DriverCheckOutcome.NotChecked)
            findings.Add(new(FindingLevel.Unknown, "Could not check this driver against the vulnerable-driver list"));
    }
}
