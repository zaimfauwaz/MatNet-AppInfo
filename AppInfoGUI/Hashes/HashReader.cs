using System.IO;
using System.Security.Cryptography;
using AppInfoGUI.Info;

namespace AppInfoGUI.Hashes;

// Calculates all the hashes in one pass over the file
public static class HashReader
{
    public static List<DetailRow> Read(string path)
    {
        try
        {
            using var md5 = IncrementalHash.CreateHash(HashAlgorithmName.MD5);
            using var sha1 = IncrementalHash.CreateHash(HashAlgorithmName.SHA1);
            using var sha256 = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            using var sha384 = IncrementalHash.CreateHash(HashAlgorithmName.SHA384);
            using var sha512 = IncrementalHash.CreateHash(HashAlgorithmName.SHA512);
            
            using var stream = File.OpenRead(path);
            byte[] buffer = new byte[1024 * 1024];
            int read;

            while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                md5.AppendData(buffer, 0, read);
                sha1.AppendData(buffer, 0, read);
                sha256.AppendData(buffer, 0, read);
                sha384.AppendData(buffer, 0, read);
                sha512.AppendData(buffer, 0, read);
            }

            return new List<DetailRow>
            {
                new("SHA-256", Hex(sha256)),
                new("SHA-384", Hex(sha384)),
                new("SHA-512", Hex(sha512)),
                new("SHA-1 (old)", Hex(sha1)),
                new("MD5 (old)", Hex(md5))
            };
        }
        catch (Exception ex)
        {
            return new List<DetailRow> { new("Hashes", "Could not read the file: " + ex.Message) };
        }
    }

    static string Hex(IncrementalHash hash) => Convert.ToHexString(hash.GetHashAndReset());
}
