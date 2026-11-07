using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace AppInfoGUI.PE;

internal static class PeText
{
    // Reads a zero-ended text that sits at a position (RVA) inside the file
    public static string ReadName(PEReader pe, int address)
    {
        BlobReader reader = pe.GetSectionData(address).GetReader();
        int length = reader.IndexOf(0);
        return reader.ReadUTF8(length < 0 ? reader.RemainingBytes : length);
    }
}
