namespace AppInfoGUI.Package.Nsis;

// Walks the installer's instructions and collects the files it will write to disk.
// It reads only. No instruction is carried out.
public static class NsisFileList
{
    const int CreateDirectory = 11;   // also what "SetOutPath" turns into
    const int ExtractFile = 20;

    public static List<PackageEntry> Read(NsisHeader header)
    {
        var names = Enumerable.Range(0, header.EntryCount)
            .Where(i => header.Opcode(i) == ExtractFile)
            .Select(i => header.Argument(i, 1));

        var strings = new NsisStrings(header, NsisStrings.LooksUnicode(header, names));

        var files = new List<PackageEntry>();
        var seen = new HashSet<(string Path, int Data)>();   // a script can extract the same file in several places
        string folder = "$INSTDIR";   // where files go until the script says otherwise

        for (int i = 0; i < header.EntryCount; i++)
        {
            int opcode = header.Opcode(i);

            // SetOutPath is a "create directory" instruction whose second argument is 1
            if (opcode == CreateDirectory && header.Argument(i, 1) == 1)
                folder = strings.Read(header.Argument(i, 0));

            if (opcode != ExtractFile) continue;

            string path = Join(folder, strings.Read(header.Argument(i, 1)));
            if (seen.Add((path, header.Argument(i, 2)))) files.Add(Entry(path));
        }

        return files;
    }

    // The format keeps file sizes inside the packed data, so they are left blank
    static PackageEntry Entry(string path) => new(path, -1, EntryClassifier.Classify(path), Encrypted: false);

    // A name that starts with a variable or a drive is already complete
    static string Join(string folder, string name) =>
        name.StartsWith('$') || name.Contains(":\\") ? name : folder.TrimEnd('\\') + "\\" + name;
}
