using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace AppInfoGUI.Crypto
{
    public static class SignatureReader
    {
        public static X509Certificate2? GetSigner(string path)
        {
            try
            {
#pragma warning disable SYSLIB0057
                var certificate = X509Certificate.CreateFromSignedFile(path);
#pragma warning restore SYSLIB0057
                return new X509Certificate2(certificate);
            }

            catch (CryptographicException)
            {
                return null;
            }
        }
    }
}
