using System.IO;

namespace AppInfoGUI.Info;

// What kind of file this is. Worked out once from the extension (and, for programs, the first bytes),
// so no other code repeats the lists.
public class FileKind
{
    public string Extension { get; }
    public ContentType Content { get; }     // from the first bytes. Unknown if the file cannot be read.

    public FileKind(string path)
    {
        Extension = Path.GetExtension(path).ToLowerInvariant();

        try
        {
            Content = FileContent.Detect(path).Type;
        }
        catch (Exception)
        {
            Content = ContentType.Unknown;
        }
    }

    // A program renamed to something else is still a program
    public bool IsProgram => Extension is ".exe" or ".scr"
        || (Content == ContentType.Program && !IsInstaller && !IsLibrary && !IsDriver);
    public bool IsMsi => Extension == ".msi";
    public bool IsInstaller => Extension is ".msi" or ".msp";
    public bool IsLibrary => Extension is ".dll" or ".rll";
    public bool IsDriver => Extension is ".sys" or ".ocx";

    // The name of the first tab
    public string DetailsTabTitle
    {
        get
        {
            if (IsInstaller) return "Installer Details";
            if (IsLibrary) return "Library Information";
            if (IsDriver) return "Driver Details";
            return "Application Details";   // .exe and anything else
        }
    }
}
