using System.Formats.Asn1;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace AppInfoGUI.Crypto.Cms;

// The outer layer of a CMS (PKCS #7) signed message: its certificates, its content and its signers.
internal class CmsEnvelope
{
    public List<X509Certificate2> Certificates { get; } = new();
    public byte[]? Content { get; private set; }        // the signed content, when it is stored as plain bytes
    public AsnReader SignerInfos { get; private set; } = null!;

    public static CmsEnvelope Parse(byte[] data)
    {
        var envelope = new CmsEnvelope();

        AsnReader message = new AsnReader(data, AsnEncodingRules.BER).ReadSequence();
        message.ReadObjectIdentifier();                                     // "signed data"
        AsnReader body = message.ReadSequence(Context(0)).ReadSequence();

        body.ReadInteger();                                                 // version
        body.ReadSetOf();                                                   // digest algorithms
        envelope.Content = ReadContent(body.ReadSequence());

        if (body.PeekTag().HasSameClassAndValue(Context(0)))
            envelope.ReadCertificates(body.ReadSetOf(Context(0)));
        if (body.PeekTag().HasSameClassAndValue(Context(1)))
            body.ReadEncodedValue();                                        // revocation lists, not needed

        envelope.SignerInfos = body.ReadSetOf();
        return envelope;
    }

    public static Asn1Tag Context(int number) => new(TagClass.ContextSpecific, number);

    // A timestamp keeps its content as bytes. A program signature keeps a structure there, which is skipped.
    static byte[]? ReadContent(AsnReader contentInfo)
    {
        contentInfo.ReadObjectIdentifier();
        if (!contentInfo.HasData) return null;

        AsnReader content = contentInfo.ReadSequence(Context(0));
        return content.PeekTag().HasSameClassAndValue(Asn1Tag.PrimitiveOctetString) ? content.ReadOctetString() : null;
    }

    void ReadCertificates(AsnReader set)
    {
        while (set.HasData)
        {
            try
            {
                Certificates.Add(X509CertificateLoader.LoadCertificate(set.ReadEncodedValue().ToArray()));
            }
            catch (CryptographicException)
            {
                // a certificate in a format we do not read: skip it
            }
        }
    }
}
