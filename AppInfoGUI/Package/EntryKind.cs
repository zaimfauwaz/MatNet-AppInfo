using System.IO;

namespace AppInfoGUI.Package;

public enum EntryKind
{
    Folder,
    Program,    // .exe, .dll and similar
    Script,     // .bat, .vbs, .ps1, shortcuts and similar
    Disguised,  // a program or script made to look like a document, such as invoice.pdf.exe
    Other
}

internal static class EntryClassifier
{
    static readonly string[] Programs = { ".exe", ".dll", ".sys", ".scr", ".com", ".msi", ".cpl", ".ocx" };
    static readonly string[] Scripts = { ".bat", ".cmd", ".ps1", ".vbs", ".vbe", ".js", ".jse", ".wsf", ".hta", ".lnk" };
    static readonly string[] Decoys = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt",
                                         ".jpg", ".jpeg", ".png", ".gif", ".mp3", ".mp4", ".zip", ".rar" };

    public static EntryKind Classify(string path)
    {
        if (path.EndsWith('/') || path.EndsWith('\\')) return EntryKind.Folder;

        string name = path.Substring(path.LastIndexOfAny(new[] { '/', '\\' }) + 1).ToLowerInvariant();
        string extension = Path.GetExtension(name);
        string inner = Path.GetExtension(Path.GetFileNameWithoutExtension(name));

        bool runnable = Programs.Contains(extension) || Scripts.Contains(extension);
        if (runnable && Decoys.Contains(inner)) return EntryKind.Disguised;

        if (Programs.Contains(extension)) return EntryKind.Program;
        if (Scripts.Contains(extension)) return EntryKind.Script;
        return EntryKind.Other;
    }
}
