using AppInfoGUI.Crypto;

namespace AppInfoGUI.Dependencies;

// Runs the same signature check as the Digital Signatures tab on one DLL
internal static class DependencySignature
{
    public const string NotChecked = "Not checked";

    public static string Check(string path)
    {
        try
        {
            SignatureResult result = SignatureVerifier.Check(path);

            if (result.IsValid)
                return result.Publisher == "Not available" ? "Valid" : "Valid - " + result.Publisher;

            if (result.SignatureType == "None")
                return "Not signed";

            return "WARNING: " + result.Verdict;
        }
        catch (Exception)
        {
            return "Could not check";
        }
    }
}
