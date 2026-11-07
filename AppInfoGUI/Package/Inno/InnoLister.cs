using System.IO;
using System.Reflection.PortableExecutable;

namespace AppInfoGUI.Package.Inno;

// Lists the files inside an Inno Setup installer by reading its setup data. Nothing is unpacked or run.
internal class InnoLister : IPackageLister
{
    public List<PackageEntry> List(Stream file, Overlay overlay)
    {
        using var pe = new PEReader(file, PEStreamOptions.LeaveOpen);

        InnoOffsetTable table = InnoOffsetTable.Find(pe, file)
            ?? throw new InvalidDataException("the Inno Setup table was not found");
        if (table.SetupDataStart <= 0)
            throw new NotSupportedException($"this Inno Setup loader version ({table.Version}) is not supported yet");

        string id = InnoSetupData.ReadId(file, table.SetupDataStart);
        InnoLayout layout = InnoLayouts.Find(id)
            ?? throw new NotSupportedException($"this version of Inno Setup is not supported yet ({id})");

        long firstBlock = InnoSetupData.FirstBlockStart(file, table.SetupDataStart, layout);
        using InnoBlock entries = InnoBlock.Open(file, firstBlock, layout.WideBlockSize);
        using InnoBlock data = InnoBlock.Open(file, entries.End, layout.WideBlockSize);

        return InnoFileList.Read(new InnoReader(entries.Data), new InnoReader(data.Data), layout);
    }
}
