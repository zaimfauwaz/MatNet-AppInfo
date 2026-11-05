
namespace AppInfoGUI.Crypto
{
    public class SignatureResult
    {
        public string Verdict { get; set; } = "";
        public bool IsValid { get; set; }
        public string SignatureType { get; set; } = "";
        public string Publisher { get; set; } = "Not available";
        public string Issuer { get; set; } = "Not available";
        public string Thumbprint { get; set; } = "Not available";
        public string ValidFrom { get; set; } = "Not available";
        public string ValidTo { get; set; } = "Not available";
        public string CatalogFile { get; set; } = "Not available";
    }
}
