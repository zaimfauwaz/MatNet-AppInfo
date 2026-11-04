using System.IO;
using System.Security.Cryptography;

namespace AppInfoGUI
{
    public static class FileHasher
    {
        public static string GetSha256(string Path)
        {
            using var fileStream = File.OpenRead(Path);
            byte[] hashBytes = SHA256.HashData(fileStream);
            return Convert.ToHexString(hashBytes);
        }
    }
}
