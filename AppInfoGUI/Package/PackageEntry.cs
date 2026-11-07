using AppInfoGUI.Info;

namespace AppInfoGUI.Package;

// One file inside a package
public record PackageEntry(string Path, long Size, EntryKind Kind, bool Encrypted)
{
    // A size below zero means the format does not say
    public string SizeText => Kind == EntryKind.Folder || Size < 0 ? "" : TextFormat.Size(Size);

    public string TypeText => (Kind switch
    {
        EntryKind.Folder => "Folder",
        EntryKind.Program => "Program or library",
        EntryKind.Script => "Script or shortcut",
        EntryKind.Disguised => "Disguised as a document",
        _ => "File"
    }) + (Encrypted ? " (password-protected)" : "");
}
