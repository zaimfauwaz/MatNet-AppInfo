using System.Formats.Asn1;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace AppInfoGUI.Crypto.Cms;

// Reads the signatures out of the signature block of a file. Only reads: nothing here decides if a signature is good.
internal static class CmsReader
{
    const string SigningTimeOid = "1.2.840.113549.1.9.5";
    const string CountersignatureOid = "1.2.840.113549.1.9.6";     // the older kind of timestamp
    const string TimestampTokenOid = "1.3.6.1.4.1.311.3.3.1";      // the newer kind (RFC 3161)
    const string NestedSignatureOid = "1.3.6.1.4.1.311.2.4.1";     // a second signature inside the first

    record Signer(X509Certificate2? Certificate, string DigestOid, DateTime? SigningTime, AsnReader? UnsignedAttributes);

    // The signature itself, then every signature nested inside it, in the order Windows numbers them
    public static List<CmsSignature> Read(byte[] message)
    {
        var all = new List<CmsSignature>();
        AddSignatures(message, all);
        return all;
    }

    static void AddSignatures(byte[] message, List<CmsSignature> all)
    {
        CmsEnvelope envelope = CmsEnvelope.Parse(message);
        var nested = new List<byte[]>();

        while (envelope.SignerInfos.HasData)
        {
            Signer signer = ReadSigner(envelope.SignerInfos.ReadSequence(), envelope.Certificates);
            DateTime? signedOn = null;
            string? timestampBy = null;

            AsnReader? attributes = signer.UnsignedAttributes;
            while (attributes != null && attributes.HasData)
                ReadUnsignedAttribute(attributes, envelope.Certificates, nested, ref signedOn, ref timestampBy);

            all.Add(new CmsSignature(signer.Certificate, CmsSignature.DigestName(signer.DigestOid), signedOn, timestampBy));
        }

        foreach (byte[] inner in nested)
            AddSignatures(inner, all);
    }

    static void ReadUnsignedAttribute(AsnReader attributes, List<X509Certificate2> certificates, List<byte[]> nested,
        ref DateTime? signedOn, ref string? timestampBy)
    {
        AsnReader attribute = attributes.ReadSequence();
        string oid = attribute.ReadObjectIdentifier();
        AsnReader values = attribute.ReadSetOf();

        try
        {
            if (oid == CountersignatureOid)
            {
                Signer stamp = ReadSigner(values.ReadSequence(), certificates);
                (signedOn, timestampBy) = (stamp.SigningTime, NameOf(stamp.Certificate));
            }
            else if (oid == TimestampTokenOid)
            {
                (signedOn, timestampBy) = ReadTimestampToken(values.ReadEncodedValue().ToArray());
            }
            else if (oid == NestedSignatureOid)
            {
                while (values.HasData)
                    nested.Add(values.ReadEncodedValue().ToArray());
            }
        }
        catch (Exception ex) when (ex is AsnContentException or CryptographicException)
        {
            // a timestamp we cannot read: the signature is still listed, without a time
        }
    }

    static Signer ReadSigner(AsnReader info, List<X509Certificate2> certificates)
    {
        info.ReadInteger();                                                     // version
        X509Certificate2? certificate = ReadSignerCertificate(info, certificates);
        string digest = info.ReadSequence().ReadObjectIdentifier();             // digest algorithm

        DateTime? signingTime = null;
        if (info.PeekTag().HasSameClassAndValue(CmsEnvelope.Context(0)))
            signingTime = FindSigningTime(info.ReadSetOf(CmsEnvelope.Context(0)));

        info.ReadEncodedValue();                                                // signature algorithm
        info.ReadEncodedValue();                                                // the signature bytes

        AsnReader? unsignedAttributes = info.HasData ? info.ReadSetOf(CmsEnvelope.Context(1)) : null;
        return new Signer(certificate, digest, signingTime, unsignedAttributes);
    }

    // The signer is named by the certificate's issuer and serial number
    static X509Certificate2? ReadSignerCertificate(AsnReader info, List<X509Certificate2> certificates)
    {
        if (!info.PeekTag().HasSameClassAndValue(Asn1Tag.Sequence))
        {
            info.ReadEncodedValue();    // named by key id instead: not handled
            return null;
        }

        AsnReader id = info.ReadSequence();
        ReadOnlyMemory<byte> issuer = id.ReadEncodedValue();
        byte[] serial = WithoutLeadingZeros(id.ReadIntegerBytes().ToArray());

        return certificates.FirstOrDefault(c =>
            c.IssuerName.RawData.AsSpan().SequenceEqual(issuer.Span)
            && WithoutLeadingZeros(c.SerialNumberBytes.ToArray()).AsSpan().SequenceEqual(serial));
    }

    static DateTime? FindSigningTime(AsnReader attributes)
    {
        while (attributes.HasData)
        {
            AsnReader attribute = attributes.ReadSequence();
            string oid = attribute.ReadObjectIdentifier();
            AsnReader values = attribute.ReadSetOf();

            if (oid != SigningTimeOid) continue;

            return values.PeekTag().HasSameClassAndValue(new Asn1Tag(UniversalTagNumber.UtcTime))
                ? values.ReadUtcTime().UtcDateTime
                : values.ReadGeneralizedTime().UtcDateTime;
        }

        return null;
    }

    // The newer timestamp is a small signed message of its own. Its content holds the time.
    static (DateTime? Time, string? By) ReadTimestampToken(byte[] token)
    {
        CmsEnvelope envelope = CmsEnvelope.Parse(token);
        if (envelope.Content == null) return (null, null);

        AsnReader info = new AsnReader(envelope.Content, AsnEncodingRules.BER).ReadSequence();
        info.ReadInteger();                 // version
        info.ReadObjectIdentifier();        // policy
        info.ReadEncodedValue();            // what was stamped
        info.ReadInteger();                 // serial number
        DateTime time = info.ReadGeneralizedTime().UtcDateTime;

        Signer authority = ReadSigner(envelope.SignerInfos.ReadSequence(), envelope.Certificates);
        return (time, NameOf(authority.Certificate));
    }

    static string? NameOf(X509Certificate2? certificate) => certificate?.GetNameInfo(X509NameType.SimpleName, false);

    static byte[] WithoutLeadingZeros(byte[] bytes) => bytes.SkipWhile(b => b == 0).ToArray();
}
