using System.IO;

namespace AppInfoGUI.Package.Nsis;

// Lists the files inside an NSIS installer. The script header gives the names. The sizes, and the
// archives inside, come from reading the installer's data once. Nothing is written or run.
internal class NsisLister : IPackageLister
{
    public List<PackageEntry> List(Stream file, Overlay overlay)
    {
        NsisFirstHeader first = NsisFirstHeader.Find(file, overlay)
            ?? throw new InvalidDataException("the NSIS data block was not found");

        NsisHeaderData header = NsisHeaderReader.Read(file, first);
        List<NsisFile> files = NsisFileList.Read(new NsisHeader(header.Bytes));

        NsisPayload payload = NsisPayload.Create(file, first, header);
        Dictionary<int, NsisFileInfo> infos = NsisInspector.Inspect(payload, files, out string? failure);

        var entries = new List<PackageEntry>();
        foreach (NsisFile item in files)
        {
            infos.TryGetValue(item.DataOffset, out NsisFileInfo? info);
            entries.Add(new PackageEntry(item.Path, info?.Size ?? -1, EntryClassifier.Classify(item.Path), Encrypted: false));
            if (info != null) entries.AddRange(info.Nested);
        }

        if (failure != null)
            entries.Add(Archives.NestedArchive.Note($"(the sizes of some files could not be read: {failure})") with { Depth = 0 });

        return entries;
    }
}
