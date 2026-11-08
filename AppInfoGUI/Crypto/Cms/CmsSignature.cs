using System.Security.Cryptography.X509Certificates;

namespace AppInfoGUI.Crypto.Cms;

// One signature found in a file: who signed, with which digest, and when (if a timestamp says so).
internal record CmsSignature(X509Certificate2? Certificate, string DigestAlgorithm, DateTime? SignedOn, string? TimestampBy)
{
    public static string DigestName(string oid) => oid switch
    {
        "1.2.840.113549.2.5" => "MD5",
        "1.3.14.3.2.26" => "SHA-1",
        "2.16.840.1.101.3.4.2.1" => "SHA-256",
        "2.16.840.1.101.3.4.2.2" => "SHA-384",
        "2.16.840.1.101.3.4.2.3" => "SHA-512",
        _ => oid
    };
}
