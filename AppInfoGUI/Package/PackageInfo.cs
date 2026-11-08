namespace AppInfoGUI.Package;

// What a package holds, ready to show
public class PackageInfo
{
    public required PackageFormat Format { get; init; }
    public long ExtraBytes { get; init; }                   // size of the data attached to the program
    public string Message { get; set; } = "";
    public List<PackageEntry> Entries { get; } = new();     // capped for display
    public int Total { get; set; }                          // how many files there really are

    // Counted over every file, not only the ones shown
    public int Programs { get; set; }
    public int Scripts { get; set; }
    public int Disguised { get; set; }
    public int Encrypted { get; set; }
    public bool Listed { get; set; }
    public bool Cancelled { get; set; }                     // the person stopped the reading
}
