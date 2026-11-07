using System.IO;
using System.Reflection.PortableExecutable;

namespace AppInfoGUI.Package.Wix;

// The ".wixburn" section of a WiX bundle. It says where the first container (the bootstrapper's own
// files, packed as a cabinet) starts and how big it is.
public record BurnHeader(long UxStart, long UxSize)
{
    const string SectionName = ".wixburn";
    const uint Magic = 0x00F14300;
    const int Cabinet = 1;           // the only container format Burn has
    const int FixedPartSize = 24 + 24;    // magic, version, id, then the six numbers before the container sizes

    // Null when the section is missing or does not look right
    public static BurnHeader? Find(PEReader pe, Stream file)
    {
        foreach (SectionHeader section in pe.PEHeaders.SectionHeaders)
        {
            if (section.Name != SectionName) continue;
            if (section.SizeOfRawData < FixedPartSize + 4) return null;

            var bytes = new byte[section.SizeOfRawData];
            file.Position = section.PointerToRawData;
            file.ReadExactly(bytes, 0, bytes.Length);

            if (BitConverter.ToUInt32(bytes, 0) != Magic) return null;
            if (BitConverter.ToUInt32(bytes, 4) != 2) return null;   // the version of this section

            uint stubSize = BitConverter.ToUInt32(bytes, 24);
            uint format = BitConverter.ToUInt32(bytes, 40);
            uint containers = BitConverter.ToUInt32(bytes, 44);
            if (format != Cabinet || containers == 0) return null;

            // The first container, right after the program itself
            uint uxSize = BitConverter.ToUInt32(bytes, FixedPartSize);
            return stubSize + (long)uxSize <= file.Length ? new BurnHeader(stubSize, uxSize) : null;
        }

        return null;
    }
}
