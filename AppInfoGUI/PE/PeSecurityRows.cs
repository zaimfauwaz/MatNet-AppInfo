using AppInfoGUI.Info;
using System.Reflection.PortableExecutable;

namespace AppInfoGUI.PE;

internal static class PeSecurityRows
{
    public static void Fill(List<DetailRow> rows, PEHeader optional)
    {
        ushort flags = (ushort)optional.DllCharacteristics;
        bool is64 = optional.Magic == PEMagic.PE32Plus;

        rows.Add(new("ASLR (random memory address)", Flag(flags, 0x0040)));
        rows.Add(new("DEP (data is not runnable)", Flag(flags, 0x0100)));
        rows.Add(new("Control Flow Guard", Flag(flags, 0x4000)));
        rows.Add(new("High-entropy ASLR", is64 ? Flag(flags, 0x0020) : "Not applicable (32-bit)"));
        rows.Add(new("Forced integrity check", Flag(flags, 0x0080)));
    }

    static string Flag(ushort flags, int mask) => (flags & mask) != 0 ? "Yes" : "No";
}
