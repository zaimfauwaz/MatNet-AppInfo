using System.IO;

namespace AppInfoGUI.Package.Nsis;

// Lists the files inside an NSIS installer by reading its script header. Nothing is unpacked or run.
internal class NsisLister : IPackageLister
{
    public List<PackageEntry> List(Stream file, Overlay overlay)
    {
        NsisFirstHeader first = NsisFirstHeader.Find(file, overlay)
            ?? throw new InvalidDataException("the NSIS data block was not found");

        var header = new NsisHeader(NsisHeaderReader.Read(file, first));
        return NsisFileList.Read(header);
    }
}
