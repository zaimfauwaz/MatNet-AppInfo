using AppInfoGUI.Package.Cab;
using System.IO;
using System.Reflection.PortableExecutable;

namespace AppInfoGUI.Package.Wix;

// Lists what a WiX bundle installs. The bootstrapper's own files are packed as a cabinet, and in it is
// the manifest that names every package. Only the manifest is unpacked, in memory. Nothing is run.
internal class WixLister : IPackageLister
{
    const string ManifestName = "0";

    public List<PackageEntry> List(Stream file, Overlay overlay)
    {
        using var pe = new PEReader(file, PEStreamOptions.LeaveOpen);

        BurnHeader header = BurnHeader.Find(pe, file)
            ?? throw new InvalidDataException("the bundle's section was not found");

        var cabinet = new CabArchive(file, header.UxStart);
        CabEntry manifest = cabinet.Entries.FirstOrDefault(e => e.Name == ManifestName)
            ?? throw new InvalidDataException("the bundle's manifest was not found");

        BurnBundle bundle = BurnManifest.Parse(cabinet.Read(manifest));
        return Rows(bundle, cabinet.Entries);
    }

    static List<PackageEntry> Rows(BurnBundle bundle, List<CabEntry> interfaceFiles)
    {
        var rows = new List<PackageEntry>();

        string? registered = Registered(bundle);
        if (registered != null) rows.Add(Note(registered, 0));

        // The place most packages share is said once. Only the packages that differ get a line of their own.
        List<string?> places = bundle.Packages.Select(p => Where(p.Payload)).ToList();
        var common = places.Where(p => p != null).GroupBy(p => p).OrderByDescending(g => g.Count()).FirstOrDefault();
        string? shared = common?.Count() >= 3 && common.Count() * 2 > places.Count ? common.Key : null;   // only when most packages agree
        if (shared != null)
        {
            string how = common!.Count() == places.Count ? "All" : $"{common.Count()} of";
            rows.Add(Note($"{how} {places.Count} packages: {shared}.", 0));
        }

        for (int i = 0; i < bundle.Packages.Count; i++)
        {
            BurnPackage package = bundle.Packages[i];
            string name = package.Payload?.FilePath ?? package.Id;
            long size = package.Size >= 0 ? package.Size : package.Payload?.FileSize ?? -1;
            rows.Add(new PackageEntry(name, size, EntryClassifier.Classify(name), Encrypted: false));

            if (places[i] != null && places[i] != shared) rows.Add(Note($"({places[i]})", 1));
        }

        if (bundle.InterfaceFiles.Count > 0)
        {
            rows.Add(Note("The bootstrapper's own files, which run first:", 0));
            foreach (BurnPayload file in bundle.InterfaceFiles)
                rows.Add(new PackageEntry(file.FilePath, SizeOf(file, interfaceFiles), EntryClassifier.Classify(file.FilePath), Encrypted: false));
        }

        return rows;
    }

    // "Windows SDK 10.1 by Microsoft Corporation", as far as the bundle says it
    static string? Registered(BurnBundle bundle)
    {
        if (bundle.Name == null && bundle.Publisher == null) return null;

        string text = $"Registered as: {bundle.Name ?? "(no name)"}";
        if (bundle.Version != null) text += $" {bundle.Version}";
        if (bundle.Publisher != null) text += $", publisher {bundle.Publisher}";
        return text;
    }

    // Some manifests leave the size out, but the cabinet knows it
    static long SizeOf(BurnPayload file, List<CabEntry> cabinetFiles) =>
        file.FileSize >= 0 ? file.FileSize : cabinetFiles.FirstOrDefault(e => e.Name == file.SourcePath)?.Size ?? -1;

    // Packages that are not inside the bundle are fetched or read from somewhere else
    static string? Where(BurnPayload? payload)
    {
        if (payload == null || payload.Packaging.Equals("embedded", StringComparison.OrdinalIgnoreCase)) return null;

        if (Uri.TryCreate(payload.DownloadUrl, UriKind.Absolute, out Uri? url))
            return $"downloaded while installing, from {url.Host}";

        return "not inside the bundle, read from beside it or fetched by the installer";
    }

    static PackageEntry Note(string text, int depth) => new(text, -1, EntryKind.Note, false, depth);
}
