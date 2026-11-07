using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using AppInfoGUI.PE;

namespace AppInfoGUI.Library;

// Reads the list of function names a DLL exports. Only names are read, not code.
internal static class ExportTable
{
    const int MaxNames = 20000;

    public static ExportInfo Read(PEReader pe)
    {
        bool isDotNet = pe.HasMetadata;
        DirectoryEntry directory = pe.PEHeaders.PEHeader!.ExportTableDirectory;
        var names = new List<string>();

        if (directory.Size == 0)
            return new ExportInfo(0, names, isDotNet);

        BlobReader table = pe.GetSectionData(directory.RelativeVirtualAddress).GetReader(0, 40);
        table.Offset = 20;   // skip the fields before the counts
        int functionCount = (int)table.ReadUInt32();
        int nameCount = (int)table.ReadUInt32();
        table.ReadUInt32();  // address of functions, not needed
        int namesAddress = (int)table.ReadUInt32();

        nameCount = Math.Min(nameCount, MaxNames);
        BlobReader addresses = pe.GetSectionData(namesAddress).GetReader(0, nameCount * 4);

        for (int i = 0; i < nameCount; i++)
        {
            int nameAddress = (int)addresses.ReadUInt32();
            names.Add(PeText.ReadName(pe, nameAddress));
        }

        return new ExportInfo(functionCount, names, isDotNet);
    }
}
