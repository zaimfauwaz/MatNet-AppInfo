using AppInfoGUI.Info;

namespace AppInfoGUI.Crypto;

// The signature details as rows for the Digital Signatures tab
internal static class SignatureRows
{
    public static List<DetailRow> From(SignatureResult r) => new()
    {
        new("Signature type", r.SignatureType),
        new("Catalog file", r.CatalogFile),
        new("Publisher", r.Publisher),
        new("Subject", r.Subject),
        new("Issuer", r.Issuer),
        new("Chain", r.Chain),
        new("Thumbprint", r.Thumbprint),
        new("Serial number", r.SerialNumber),
        new("Algorithm", r.Algorithm),
        new("Valid from", r.ValidFrom),
        new("Valid to", r.ValidTo),
        new("Expiry", r.Expiry),
        new("Extended Validation", r.Validation)
    };
}
