using AppInfoGUI.Hashes;
using AppInfoGUI.Crypto;

namespace AppInfoGUI.Driver;

public enum DriverCheckOutcome
{
    NotApplicable,   // not a driver file
    Match,           // a known vulnerable driver
    NoMatch,         // not in the list (which does not prove it is safe)
    NotChecked       // there was no list, or the check failed
}

public record DriverCheckResult(DriverCheckOutcome Outcome, string Text);

// Compares a driver with the list of known vulnerable drivers. Nothing leaves this PC.
internal static class VulnerableDriverCheck
{
    public static DriverCheckResult Run(string path)
    {
        try
        {
            return Run(path, VulnerableDriverList.Open());
        }
        catch (Exception ex)
        {
            return new(DriverCheckOutcome.NotChecked, "Could not check: " + ex.Message);
        }
    }

    public static DriverCheckResult Run(string path, VulnerableDriverList? list)
    {
        // No list means we did not check. That must never look like a pass.
        if (list == null || list.Count == 0)
            return new(DriverCheckOutcome.NotChecked,
                "Not checked - no driver list found. Run Scripts\\Update-VulnerableDrivers.ps1 to download one.");

        string? byFile = list.FindByFileHash(FileHasher.GetSha256(path));
        if (byFile != null)
            return new(DriverCheckOutcome.Match, $"YES - this exact file is a known vulnerable driver ({byFile})");

        // The same driver, re-signed or with a changed signature, still has the same signed-content hash
        string? bySigned = list.FindBySignedHash(AuthenticodeHash.Sha256(path));
        if (bySigned != null)
            return new(DriverCheckOutcome.Match, $"YES - same driver code as a known vulnerable driver ({bySigned})");

        return new(DriverCheckOutcome.NoMatch,
            $"No match in the list of {list.Count} known hashes (list dated {list.Dated:yyyy-MM-dd}). "
            + "Not being listed does not prove a driver is safe.");
    }
}
