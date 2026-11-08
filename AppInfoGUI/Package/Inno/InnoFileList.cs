using System.IO;

namespace AppInfoGUI.Package.Inno;

// Reads the entries of the setup data in their fixed order and keeps the files.
public static class InnoFileList
{
    const int MaxEntries = 1_000_000;
    const int DestinationName = 1;     // the second string of a file entry
    const int SourceName = 0;

    // `reader` reads the first block (the entries), `dataReader` the second (the file sizes)
    public static List<PackageEntry> Read(InnoReader reader, InnoReader dataReader, InnoLayout layout)
    {
        // The header's strings, then a block that starts with the number of entries of each kind
        (_, byte[] header) = reader.Entry(layout.Header);
        int[] counts = Counts(header, layout);

        Skip(reader, layout.Language, counts[0]);
        Skip(reader, layout.CustomMessage, counts[1]);
        Skip(reader, layout.Permission, counts[2]);
        Skip(reader, layout.Type, counts[3]);
        Skip(reader, layout.Component, counts[4]);
        Skip(reader, layout.Task, counts[5]);
        Skip(reader, layout.Directory, counts[6]);

        int next = 7;
        if (layout.HasSigningKeys) Skip(reader, layout.SigningKey, counts[next++]);

        int fileCount = counts[next++];
        int dataCount = counts[next++];

        var files = new List<(string Source, string Destination, int Location)>();
        for (int i = 0; i < fileCount; i++)
        {
            (string[] strings, byte[] tail) = reader.Entry(layout.File);
            files.Add((strings[SourceName], strings[DestinationName], BitConverter.ToInt32(tail, layout.LocationNumberAt)));
        }

        // The other entries are only skipped. Their number is counted in the header.
        Skip(reader, layout.Icon, counts[next++]);
        Skip(reader, layout.Ini, counts[next++]);
        Skip(reader, layout.Registry, counts[next++]);
        Skip(reader, layout.Delete, counts[next++]);   // delete on install
        Skip(reader, layout.Delete, counts[next++]);   // delete on uninstall
        Skip(reader, layout.Run, counts[next++]);
        Skip(reader, layout.Run, counts[next]);        // run on uninstall

        // The second block holds one data entry per stored file, with its size
        var sizes = new long[dataCount];
        for (int i = 0; i < dataCount; i++)
            sizes[i] = BitConverter.ToInt64(dataReader.Bytes(layout.LocationTail), layout.LocationSizeAt);

        return files
            .Select(f => Entry(Path(f.Source, f.Destination), f.Location >= 0 && f.Location < dataCount ? sizes[f.Location] : -1))
            .OfType<PackageEntry>()
            .ToList();
    }

    // The entry counts sit at the start of the header's block, in a fixed order
    static int[] Counts(byte[] header, InnoLayout layout)
    {
        int kinds = layout.HasSigningKeys ? 17 : 16;
        var counts = new int[kinds];

        for (int i = 0; i < kinds; i++)
        {
            counts[i] = BitConverter.ToInt32(header, i * 4);
            if (counts[i] < 0 || counts[i] > MaxEntries) throw new InvalidDataException("the Inno Setup data is damaged");
        }

        return counts;
    }

    static void Skip(InnoReader reader, InnoShape shape, int count)
    {
        for (int i = 0; i < count; i++) reader.Entry(shape);
    }

    // A destination ending in "\" is a folder. The file keeps the name it has at the source.
    static string Path(string source, string destination)
    {
        if (!destination.EndsWith('\\')) return destination;

        string name = source.Substring(source.LastIndexOfAny(new[] { '\\', '/' }) + 1);
        return name.Length == 0 ? "" : destination + name;
    }

    // Entries with no file name only create a folder
    static PackageEntry? Entry(string path, long size) =>
        path.Length == 0 ? null : new PackageEntry(path, size, EntryClassifier.Classify(path), Encrypted: false);
}
