using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using AppInfoGUI.PE;

namespace AppInfoGUI.Dependencies;

// Reads the list of DLLs and function names a file imports. Only names are read, not code.
internal static class ImportTable
{
    const int MaxDlls = 2000;
    const int MaxFunctions = 5000;

    public record Import(string Dll, List<string> Functions);

    public static List<Import> Read(PEReader pe)
    {
        var imports = new List<Import>();
        PEHeader header = pe.PEHeaders.PEHeader!;
        bool is64 = header.Magic == PEMagic.PE32Plus;

        DirectoryEntry directory = header.ImportTableDirectory;
        if (directory.Size == 0) return imports;

        // One 20-byte entry per DLL, ending with an empty entry
        BlobReader table = pe.GetSectionData(directory.RelativeVirtualAddress).GetReader();

        for (int i = 0; i < MaxDlls; i++)
        {
            int namesAddress = (int)table.ReadUInt32();   // the list of function names
            table.ReadUInt32();                            // time stamp
            table.ReadUInt32();                            // forwarder
            int dllNameAddress = (int)table.ReadUInt32();
            int fallbackAddress = (int)table.ReadUInt32(); // used when the first one is empty

            if (dllNameAddress == 0) break;

            string dll = PeText.ReadName(pe, dllNameAddress);
            int listAddress = namesAddress != 0 ? namesAddress : fallbackAddress;
            imports.Add(new Import(dll, ReadFunctions(pe, listAddress, is64)));
        }

        return imports;
    }

    static List<string> ReadFunctions(PEReader pe, int listAddress, bool is64)
    {
        var functions = new List<string>();
        BlobReader list = pe.GetSectionData(listAddress).GetReader();

        for (int i = 0; i < MaxFunctions; i++)
        {
            ulong entry = is64 ? list.ReadUInt64() : list.ReadUInt32();
            if (entry == 0) break;

            bool byNumber = (entry >> (is64 ? 63 : 31)) == 1;
            if (byNumber)
                functions.Add("Ordinal #" + (entry & 0xFFFF));
            else
                functions.Add(PeText.ReadName(pe, (int)(entry & 0x7FFFFFFF) + 2));   // skip the 2-byte hint
        }

        return functions;
    }
}
