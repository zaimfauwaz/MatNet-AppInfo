namespace AppInfoGUI.Package.Inno;

// What the setup data of one Inno Setup format version looks like.
// Only the facts needed to reach the list of files are kept here.
public record InnoLayout(string SetupId)
{
    // These entries are stored the same way in every supported version
    public InnoShape Language { get; init; } = new(6, 4, 21);
    public InnoShape CustomMessage { get; init; } = new(2, 0, 4);
    public InnoShape Permission { get; init; } = new(0, 1, 0);
    public InnoShape Type { get; init; } = new(4, 0, 30);
    public InnoShape Component { get; init; } = new(5, 0, 42);
    public InnoShape Task { get; init; } = new(6, 0, 26);
    public InnoShape Directory { get; init; } = new(7, 0, 27);
    public InnoShape SigningKey { get; init; } = new(3, 0, 0);
    public InnoShape Ini { get; init; } = new(10, 0, 21);
    public InnoShape Registry { get; init; } = new(9, 0, 29);
    public InnoShape Delete { get; init; } = new(7, 0, 21);
    public InnoShape Run { get; init; } = new(13, 0, 27);
    public InnoShape Icon { get; init; } = new(13, 0, 48);

    // These change from version to version (the entries are listed in the order they are stored)
    public required InnoShape Header { get; init; }
    public required InnoShape File { get; init; }
    public required int LocationNumberAt { get; init; }    // where a file entry holds the number of its data entry
    public required int LocationTail { get; init; }        // size of a data entry (it has no strings)
    public required int LocationSizeAt { get; init; }      // where a data entry holds the file's size

    public bool HasSigningKeys { get; init; }              // Inno Setup 6.5 and later
    public bool HasEncryptionHeader { get; init; }         // Inno Setup 6.5 and later
    public bool NeedsUnicodeMark { get; init; }            // 6.0 and 6.1 also came as ANSI builds, which are not supported
}
