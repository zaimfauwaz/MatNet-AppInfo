
namespace AppInfoGUI.Crypto
{
    public class SignatureResult
    {
        const string NotAvailable = "Not available";

        public string Verdict { get; set; } = "";
        public bool IsValid { get; set; }
        public string SignatureType { get; set; } = "";
        public string Publisher { get; set; } = NotAvailable;
        public string Issuer { get; set; } = NotAvailable;
        public string Thumbprint { get; set; } = NotAvailable;
        public string ValidFrom { get; set; } = NotAvailable;
        public string ValidTo { get; set; } = NotAvailable;
        public string CatalogFile { get; set; } = NotAvailable;
        public string Subject { get; set; } = NotAvailable;
        public string SerialNumber { get; set; } = NotAvailable;
        public string Algorithm { get; set; } = NotAvailable;
        public string Expiry { get; set; } = NotAvailable;
        public string Chain { get; set; } = NotAvailable;
        public string Validation { get; set; } = NotAvailable;

        // Every signature in the file, when it carries its own. Empty if they could not be read.
        public List<SignatureEntry> Signatures { get; } = new();
    }
}
