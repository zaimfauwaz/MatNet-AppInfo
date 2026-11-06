using System.IO;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using AppInfoGUI.Info;

namespace AppInfoGUI.Hashes;

// The hash that a signature signs. It skips the parts of the file the signature changes:
// the checksum, the certificate pointer, and the certificate itself.
public static class AuthenticodeHash
{
    public static List<DetailRow> Read(string path)
    {
        try
        {
            using var stream = File.OpenRead(path);
            using var pe = new PEReader(stream);

            PEHeaders headers = pe.PEHeaders;
            PEHeader? optional = headers.PEHeader;
            if (optional == null)
                return Failed("Not a PE file");

            // Where the two skipped fields sit in the file
            long optionalStart = headers.PEHeaderStartOffset;
            long checksumAt = optionalStart + 64;
            long certificatePointerAt = optionalStart + (optional.Magic == PEMagic.PE32Plus ? 144 : 128);
            long certificateSize = optional.CertificateTableDirectory.Size;
            long headerSize = optional.SizeOfHeaders;

            using var sha256 = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            using var sha1 = IncrementalHash.CreateHash(HashAlgorithmName.SHA1);
            IncrementalHash[] hashes = { sha256, sha1 };

            // 1. The headers, without the two skipped fields
            Append(stream, 0, checksumAt, hashes);
            Append(stream, checksumAt + 4, certificatePointerAt, hashes);
            Append(stream, certificatePointerAt + 8, headerSize, hashes);

            // 2. The sections, in the order they sit in the file
            long hashed = headerSize;
            foreach (SectionHeader section in headers.SectionHeaders.OrderBy(s => s.PointerToRawData))
            {
                if (section.SizeOfRawData == 0) continue;

                Append(stream, section.PointerToRawData, section.PointerToRawData + section.SizeOfRawData, hashes);
                hashed += section.SizeOfRawData;
            }

            // 3. Anything after the sections, except the certificate at the very end
            Append(stream, hashed, stream.Length - certificateSize, hashes);

            return new List<DetailRow>
            {
                new("Authenticode SHA-256", Convert.ToHexString(sha256.GetHashAndReset())),
                new("Authenticode SHA-1", Convert.ToHexString(sha1.GetHashAndReset()))
            };
        }
        catch (Exception ex)
        {
            return Failed("Could not calculate: " + ex.Message);
        }
    }

    // Feeds the bytes from start up to (not including) end into every hash
    static void Append(Stream stream, long start, long end, IncrementalHash[] hashes)
    {
        end = Math.Min(end, stream.Length);
        if (start >= end) return;

        stream.Position = start;
        byte[] buffer = new byte[1024 * 1024];
        long left = end - start;

        while (left > 0)
        {
            int read = stream.Read(buffer, 0, (int)Math.Min(buffer.Length, left));
            if (read == 0) break;

            foreach (IncrementalHash hash in hashes)
                hash.AppendData(buffer, 0, read);
            left -= read;
        }
    }

    static List<DetailRow> Failed(string reason) => new() { new("Authenticode hash", reason) };
}
