using AppInfoGUI.Crypto.Cms;
using System.Security.Cryptography.X509Certificates;

namespace AppInfoGUI.Crypto;

// Every signature a file carries: the details come from the file, the verdict on each comes from Windows.
internal static class SignatureList
{
    public static List<SignatureEntry> Read(string path)
    {
        var entries = new List<SignatureEntry>();

        foreach (byte[] message in CertificateTable.Read(path))
            foreach (CmsSignature signature in CmsReader.Read(message))
                entries.Add(ToEntry(signature, entries.Count + 1));

        // Windows numbers the signatures the same way. If the counts differ, none of them is matched up.
        if (EmbeddedVerifier.CountSignatures(path) == entries.Count)
        {
            foreach (SignatureEntry entry in entries)
            {
                uint code = EmbeddedVerifier.VerifyOne(path, (uint)entry.Number - 1);
                entry.WindowsChecked = true;
                entry.IsValid = code == 0;
                entry.Verdict = SignatureVerifier.Describe(code);
            }
        }

        return entries;
    }

    static SignatureEntry ToEntry(CmsSignature signature, int number)
    {
        X509Certificate2? cert = signature.Certificate;

        return new SignatureEntry
        {
            Number = number,
            Algorithm = signature.DigestAlgorithm,
            SignedOn = signature.SignedOn,
            TimestampBy = signature.TimestampBy,
            Publisher = cert?.GetNameInfo(X509NameType.SimpleName, false) ?? "Not available",
            CertificateFrom = cert?.NotBefore,
            CertificateTo = cert?.NotAfter
        };
    }
}
