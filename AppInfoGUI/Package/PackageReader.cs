using AppInfoGUI.Info;
using AppInfoGUI.Package.Streams;
using System.IO;

namespace AppInfoGUI.Package;

// Works out whether an .exe is an installer or self-extracting archive, and lists what is inside.
// Nothing is run or unpacked.
public static class PackageReader
{
    const int MaxRows = 5000;

    // Quick: only looks for the markers. A null format means a plain program.
    public static Detection Detect(string path) => PackageDetector.Detect(path);

    // Slow for big installers: lists what is inside. Call it after `Detect` found a format.
    // Cancelling stops it at the next read and gives back an info that says so.
    public static PackageInfo Read(string path, Detection detection, CancellationToken cancel = default)
    {
        PackageFormat format = detection.Format ?? throw new ArgumentException("the file is not a package");
        var info = new PackageInfo { Format = format, ExtraBytes = detection.Overlay?.Length ?? 0 };

        if (format.Lister == null || detection.Overlay == null)
        {
            info.Message = format.Name + ". Listing the files inside is not supported for this type yet.";
            return info;
        }

        try
        {
            using FileStream file = File.OpenRead(path);
            var stream = new CancellableStream(file, cancel);
            List<PackageEntry> all = format.Lister.List(stream, detection.Overlay);

            // A lister may have caught the cancel as a read problem, so check again
            cancel.ThrowIfCancellationRequested();
            Fill(info, all);
        }
        catch (OperationCanceledException)
        {
            info.Cancelled = true;
            info.Message = format.Name + ". Reading was cancelled, so the files inside are not listed.";
        }
        catch (Exception ex)
        {
            info.Message = $"{format.Name}, but its files could not be listed: {ex.Message}";
        }

        return info;
    }

    static void Fill(PackageInfo info, List<PackageEntry> all)
    {
        List<PackageEntry> files = all.Where(e => e.Kind != EntryKind.Note).ToList();

        info.Listed = true;
        info.Total = files.Count;
        info.Entries.AddRange(all.Take(MaxRows));

        info.Programs = files.Count(e => e.Kind is EntryKind.Program or EntryKind.Disguised);
        info.Scripts = files.Count(e => e.Kind == EntryKind.Script);
        info.Disguised = files.Count(e => e.Kind == EntryKind.Disguised);
        info.Encrypted = files.Count(e => e.Encrypted);

        int installers = files.Count(e => e.Kind == EntryKind.Installer);
        int nested = files.Count(e => e.Depth > 0);
        info.Message = $"{info.Format.Name}. It holds {(files.Count == 1 ? "1 file" : $"{files.Count} files")}"
                     + (nested > 0 ? $" ({nested} of them inside archives)" : "")
                     + $": {info.Programs} programs or libraries, "
                     + (installers > 0 ? $"{installers} installer packages, " : "")
                     + $"{info.Scripts} scripts or shortcuts. "
                     + "Nothing has been unpacked."
                     + (all.Count > MaxRows ? $" Showing the first {MaxRows}." : "");
    }
}
