using System.IO;

namespace AppInfoGUI.Info;

// What kind of file this is. Worked out once from the extension, so no other code repeats the lists.
public class FileKind
{
    public string Extension { get; }

    public FileKind(string path)
    {
        Extension = Path.GetExtension(path).ToLowerInvariant();
    }

    public bool IsProgram => Extension is ".exe" or ".scr";
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
