using System.IO;
using System.Text;

namespace AppInfoGUI.Package.Inno;

// The start of the setup data: an ID text, and from Inno Setup 6.5 an encryption header.
// The first packed block follows.
public static class InnoSetupData
{
    const int IdSize = 64;
    const int EncryptionHeaderSize = 4 + 49;     // a checksum, then the header
    const int FullEncryption = 2;                // the first byte of the encryption header

    // The ID text, like "Inno Setup Setup Data (6.4.3)"
    public static string ReadId(Stream file, long start)
    {
        var id = new byte[IdSize];
        file.Position = start;
        file.ReadExactly(id, 0, IdSize);

        string text = Encoding.ASCII.GetString(id).TrimEnd('\0');
        if (!text.StartsWith("Inno Setup Setup Data")) throw new InvalidDataException("the Inno Setup data was not found");
        return text;
    }

    // Where the first packed block starts
    public static long FirstBlockStart(Stream file, long start, InnoLayout layout)
    {
        long at = start + IdSize;
        if (!layout.HasEncryptionHeader) return at;

        file.Position = at + 4;
        if (file.ReadByte() == FullEncryption)
            throw new InvalidDataException("the installer's description is password protected");

        return at + EncryptionHeaderSize;
    }
}
