using AppInfoGUI.Info;

namespace AppInfoGUI.Crypto;

// The signature details as rows for the Digital Signatures tab
internal static class SignatureRows
{
    public static List<DetailRow> From(SignatureResult r)
    {
        var rows = new List<DetailRow>
        {
            new("Windows verdict", r.Verdict),
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

        AddSignatures(rows, r.Signatures, DateTime.Now);
        return rows;
    }

    // One row for each signature. The ones Windows accepts come first, since those are the ones that count.
    static void AddSignatures(List<DetailRow> rows, List<SignatureEntry> signatures, DateTime now)
    {
        if (signatures.Count == 0) return;

        rows.Add(new("Signatures in the file", Summary(signatures)));

        foreach (SignatureEntry entry in signatures.OrderBy(e => e.IsValid ? 0 : 1).ThenBy(e => e.Number))
            rows.Add(new($"Signature {entry.Number} of {signatures.Count}", Describe(entry, now)));
    }

    static string Summary(List<SignatureEntry> signatures)
    {
        if (signatures.Any(s => !s.WindowsChecked))
            return $"{signatures.Count} found (Windows could not judge each one on its own)";

        int valid = signatures.Count(s => s.IsValid);
        return $"{signatures.Count} found, {valid} valid";
    }

    static string Describe(SignatureEntry e, DateTime now)
    {
        string mark = !e.WindowsChecked ? "•" : e.IsValid ? "✔" : "✖";

        var lines = new List<string>
        {
            $"{mark} {e.Verdict}",
            $"Signed by: {e.Publisher}",
            $"Digest: {e.Algorithm}",
            $"Certificate valid: {Dates(e, now)}",
            $"Signed on: {SignedOn(e)}",
            $"Timestamp check: {TimestampCheck(e, now)}"
        };

        return string.Join("\n", lines);
    }

    static string Dates(SignatureEntry e, DateTime now)
    {
        if (e.CertificateFrom == null || e.CertificateTo == null) return "Not available";

        string text = $"{e.CertificateFrom:yyyy-MM-dd} to {e.CertificateTo:yyyy-MM-dd}";
        return e.CertificateTo < now ? text + " (expired)" : text;
    }

    static string SignedOn(SignatureEntry e)
    {
        if (e.SignedOn == null) return "No timestamp";

        string by = string.IsNullOrEmpty(e.TimestampBy) ? "" : $", timestamp by {e.TimestampBy}";
        return $"{e.SignedOn:yyyy-MM-dd HH:mm} UTC{by}";
    }

    // The timestamp is what keeps a signature good after its certificate runs out
    static string TimestampCheck(SignatureEntry e, DateTime now)
    {
        if (e.CertificateFrom == null || e.CertificateTo == null)
            return "Not available";

        bool expired = e.CertificateTo < now;

        if (e.SignedOn == null)
            return expired
                ? "No timestamp and the certificate has expired, so this signature no longer counts"
                : "No timestamp, so this signature stops counting when the certificate expires";

        DateTime signed = e.SignedOn.Value.ToLocalTime();
        if (signed < e.CertificateFrom || signed > e.CertificateTo)
            return "Signed outside the certificate's valid dates";

        return expired
            ? "Signed while the certificate was valid, so it still counts after the certificate expired"
            : "Signed while the certificate was valid";
    }
}
