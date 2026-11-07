using AppInfoGUI.Info;
using System.IO;

namespace AppInfoGUI.Package;

// Works out whether an .exe is an installer or self-extracting archive, and lists what is inside.
// Nothing is run or unpacked.
public static class PackageReader
{
    const int MaxRows = 5000;

    // Null when the file is a plain program
    public static PackageInfo? Read(string path)
    {
        Detection detection = PackageDetector.Detect(path);
        if (detection.Format == null) return null;

        var info = new PackageInfo { Format = detection.Format, ExtraBytes = detection.Overlay?.Length ?? 0 };

        if (detection.Format.Lister == null || detection.Overlay == null)
        {
            info.Message = detection.Format.Name + ". Listing the files inside is not supported for this type yet.";
            return info;
        }

        try
        {
            using FileStream stream = File.OpenRead(path);
            List<PackageEntry> all = detection.Format.Lister.List(stream, detection.Overlay);
            Fill(info, all);
        }
        catch (Exception ex)
        {
            info.Message = $"{detection.Format.Name}, but its files could not be listed: {ex.Message}";
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

        int nested = files.Count(e => e.Depth > 0);
        info.Message = $"{info.Format.Name}. It holds {(files.Count == 1 ? "1 file" : $"{files.Count} files")}"
                     + (nested > 0 ? $" ({nested} of them inside archives)" : "")
                     + $": {info.Programs} programs or libraries, {info.Scripts} scripts or shortcuts. "
                     + "Nothing has been unpacked."
                     + (all.Count > MaxRows ? $" Showing the first {MaxRows}." : "");
    }
}
