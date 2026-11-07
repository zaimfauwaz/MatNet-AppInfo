using AppInfoGUI.Info;

namespace AppInfoGUI.Package;

// One file inside a package. Depth is 1 for a file inside an archive that is itself inside the package.
public record PackageEntry(string Path, long Size, EntryKind Kind, bool Encrypted, int Depth = 0)
{
    // What the list shows: files of a nested archive are indented under it
    public string DisplayPath => Depth == 0 ? Path : new string(' ', Depth * 4) + "\u21B3 " + Path;

    // A size below zero means the format does not say
    public string SizeText => Kind is EntryKind.Folder or EntryKind.Note || Size < 0 ? "" : TextFormat.Size(Size);

    public string TypeText => (Kind switch
    {
        EntryKind.Folder => "Folder",
        EntryKind.Program => "Program or library",
        EntryKind.Script => "Script or shortcut",
        EntryKind.Disguised => "Disguised as a document",
        EntryKind.Note => "",
        _ => "File"
    }) + (Encrypted ? " (password-protected)" : "");
}
