using System.IO;

namespace AppInfoGUI.Package.Nsis;

// Lists the files inside an NSIS installer by reading its script header. Nothing is unpacked or run.
// The file sizes are not in the header, so they are left blank.
internal class NsisLister : IPackageLister
{
    public List<PackageEntry> List(Stream file, Overlay overlay)
    {
        NsisFirstHeader first = NsisFirstHeader.Find(file, overlay)
            ?? throw new InvalidDataException("the NSIS data block was not found");

        NsisHeaderData data = NsisHeaderReader.Read(file, first);

        var entries = new List<PackageEntry>();
        foreach (NsisFile item in NsisFileList.Read(new NsisHeader(data.Bytes)))
        {
            entries.Add(new PackageEntry(item.Path, -1, EntryClassifier.Classify(item.Path), Encrypted: false));
            entries.AddRange(NsisNested.List(file, data.FilesStart, item));
        }

        return entries;
    }
}
