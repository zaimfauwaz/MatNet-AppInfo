namespace AppInfoGUI.Crypto;

// One of the signatures on a file. A file can carry several, for example one with SHA-1 and one with SHA-256.
public class SignatureEntry
{
    public int Number { get; set; }                         // 1 is the first signature in the file

    // Windows' own verdict on this signature alone. If it could not be asked, the signature is not called good.
    public bool WindowsChecked { get; set; }
    public bool IsValid { get; set; }
    public string Verdict { get; set; } = "Not checked on its own";

    public string Publisher { get; set; } = "Not available";
    public string Algorithm { get; set; } = "Not available";
    public DateTime? CertificateFrom { get; set; }
    public DateTime? CertificateTo { get; set; }
    public DateTime? SignedOn { get; set; }                 // from a timestamp, UTC
    public string? TimestampBy { get; set; }
}
