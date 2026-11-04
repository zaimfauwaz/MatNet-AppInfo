namespace AppInfoGUI.Crypto;

public static class SignatureVerifier
{
    const uint NoSignature = 0x800B0100;

    public static string Verify(string path)
    {
        uint code = EmbeddedVerifier.Verify(path);

        if (code == NoSignature)
            code = CatalogVerifier.Verify(path);

        return Describe(code);
    }

    static string Describe(uint code) => code switch
    {
        0 => "Valid: signature intact and trusted",
        0x800B0100 => "Not signed (no embedded or catalog signature)",
        0x80096010 => "Signed, but the file was modified (bad digest)",
        0x800B0109 => "Signed, but the root certificate is not trusted",
        0x800B0101 => "Signed, but the certificate has expired",
        0x800B010C => "Signed, but the certificate was revoked",
        0x800B0111 => "Publisher is explicitly blocked",
        0x800B0004 => "Signed, but not trusted",
        _ => $"Other result: 0x{code:X8}"
    };
}