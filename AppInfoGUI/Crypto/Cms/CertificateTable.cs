using System.IO;
using System.Reflection.PortableExecutable;

namespace AppInfoGUI.Crypto.Cms;

// Cuts the signature block out of a program file. Each signature message is a WIN_CERTIFICATE entry.
internal static class CertificateTable
{
    const int HeaderSize = 8;           // length (4 bytes), revision (2), type (2)
    const int SignedDataType = 2;       // the entry holds a PKCS #7 message

    // Empty when the file carries no signature of its own
    public static List<byte[]> Read(string path)
    {
        var messages = new List<byte[]>();

        using FileStream stream = File.OpenRead(path);
        using var pe = new PEReader(stream);

        DirectoryEntry table = pe.PEHeaders.PEHeader?.CertificateTableDirectory ?? default;
        if (table.Size == 0) return messages;

        long position = table.RelativeVirtualAddress;   // in this one table the "address" is a position in the file
        long end = Math.Min(position + table.Size, stream.Length);
        var header = new byte[HeaderSize];

        while (position + HeaderSize <= end)
        {
            stream.Position = position;
            stream.ReadExactly(header);

            int length = BitConverter.ToInt32(header, 0);
            int type = BitConverter.ToUInt16(header, 6);
            if (length < HeaderSize || position + length > end) break;

            if (type == SignedDataType)
            {
                var message = new byte[length - HeaderSize];
                stream.ReadExactly(message);
                messages.Add(message);
            }

            position += (length + 7) & ~7;      // entries start on 8-byte boundaries
        }

        return messages;
    }
}
