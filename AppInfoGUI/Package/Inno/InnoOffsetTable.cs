using System.IO;
using System.Reflection.PortableExecutable;

namespace AppInfoGUI.Package.Inno;

// Inno Setup keeps a small table in a resource of the program. It says where the setup data starts.
public record InnoOffsetTable(uint Version, long SetupDataStart)
{
    const int MaxResourceBytes = 32 * 1024 * 1024;

    static readonly byte[] Marker = { (byte)'r', (byte)'D', (byte)'l', (byte)'P', (byte)'t', (byte)'S', 0xCD, 0xE6, 0xD7, 0x7B, 0x0B, 0x2A };

    // Null when the program is not an Inno Setup installer
    public static InnoOffsetTable? Find(PEReader pe, Stream file)
    {
        SectionHeader resources = pe.PEHeaders.SectionHeaders.FirstOrDefault(s => s.Name == ".rsrc");
        if (resources.SizeOfRawData == 0) return null;

        var bytes = new byte[Math.Min(resources.SizeOfRawData, MaxResourceBytes)];
        file.Position = resources.PointerToRawData;
        int read = file.Read(bytes, 0, bytes.Length);

        int at = bytes.AsSpan(0, read).IndexOf(Marker);
        if (at < 0 || at + 64 > read) return null;

        // Version 1 stores 32-bit positions. Version 2 (Inno Setup 6.5 and later) stores 64-bit ones.
        uint version = BitConverter.ToUInt32(bytes, at + 12);
        long start = version switch
        {
            1 => BitConverter.ToUInt32(bytes, at + 32),
            2 => BitConverter.ToInt64(bytes, at + 40),
            _ => 0
        };

        return new InnoOffsetTable(version, start);
    }
}
