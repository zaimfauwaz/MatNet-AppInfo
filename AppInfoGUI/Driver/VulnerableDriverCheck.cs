using AppInfoGUI.Hashes;
using AppInfoGUI.Crypto;

namespace AppInfoGUI.Driver;

// Compares a driver with the list of known vulnerable drivers. Nothing leaves this PC.
internal static class VulnerableDriverCheck
{
    public static string Describe(string path)
    {
        try
        {
            return Describe(path, VulnerableDriverList.Open());
        }
        catch (Exception ex)
        {
            return "Could not check: " + ex.Message;
        }
    }

    public static string Describe(string path, VulnerableDriverList? list)
    {
        // No list means we did not check. That must never look like a pass.
        if (list == null)
            return "Not checked - no driver list found. Run Scripts\\Update-VulnerableDrivers.ps1 to download one.";

        string? byFile = list.FindByFileHash(FileHasher.GetSha256(path));
        if (byFile != null)
            return $"YES - this exact file is a known vulnerable driver ({byFile})";

        // The same driver, re-signed or with a changed signature, still has the same signed-content hash
        string? bySigned = list.FindBySignedHash(AuthenticodeHash.Sha256(path));
        if (bySigned != null)
            return $"YES - same driver code as a known vulnerable driver ({bySigned})";

        return $"No match in the list of {list.Count} known hashes (list dated {list.Dated:yyyy-MM-dd}). "
             + "Not being listed does not prove a driver is safe.";
    }
}
