using AppInfoGUI.Info;
using System.Formats.Asn1;
using System.Security.Cryptography.X509Certificates;

namespace AppInfoGUI.Crypto;

public static class CertificateDetails
{
    const string EvCodeSigningPolicy = "2.23.140.1.3";
    const string CertificatePoliciesId = "2.5.29.32";

    public static void Fill(SignatureResult result, X509Certificate2 cert)
    {
        result.Publisher = Safe.Text(() => cert.GetNameInfo(X509NameType.SimpleName, false));
        result.Subject = Safe.Text(() => cert.Subject);
        result.Issuer = Safe.Text(() => cert.GetNameInfo(X509NameType.SimpleName, true));
        result.Chain = Safe.Text(() => BuildChainText(cert));
        result.Thumbprint = Safe.Text(() => cert.Thumbprint);
        result.SerialNumber = Safe.Text(() => cert.SerialNumber);
        result.Algorithm = Safe.Text(() => cert.SignatureAlgorithm.FriendlyName ?? cert.SignatureAlgorithm.Value);
        result.ValidFrom = Safe.Text(() => cert.NotBefore.ToString("yyyy-MM-dd"));
        result.ValidTo = Safe.Text(() => cert.NotAfter.ToString("yyyy-MM-dd"));
        result.Expiry = Safe.Text(() => DescribeExpiry(cert));
        result.Validation = Safe.Text(() => DescribeValidation(cert));
    }

    static string DescribeExpiry(X509Certificate2 cert)
    {
        DateTime now = DateTime.Now;

        if (now < cert.NotBefore) return "Not valid yet";
        if (now > cert.NotAfter) return $"Expired {(now - cert.NotAfter).Days} days ago";
        return $"{(cert.NotAfter - now).Days} days left";
    }

    static string DescribeValidation(X509Certificate2 cert)
    {
        foreach (X509Extension extension in cert.Extensions)
        {
            if (extension.Oid?.Value != CertificatePoliciesId) continue;

            var policies = new AsnReader(extension.RawData, AsnEncodingRules.DER).ReadSequence();
            while (policies.HasData)
            {
                string policyId = policies.ReadSequence().ReadObjectIdentifier();
                if (policyId == EvCodeSigningPolicy)
                    return "EV policy found";
            }

            return "EV policy not found";
        }

        return "Not available";
    }

    static string BuildChainText(X509Certificate2 cert)
    {
        using var chain = new X509Chain();
        chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
        chain.Build(cert);

        var names = new List<string>();
        foreach (X509ChainElement element in chain.ChainElements)
            names.Add(element.Certificate.GetNameInfo(X509NameType.SimpleName, false));

        return string.Join("  >  ", names);
    }
}