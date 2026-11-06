using System.IO;
using AppInfoGUI.Info;

namespace AppInfoGUI.Library;

// A DLL named like a Windows system DLL but kept somewhere else is a known trick
internal static class SystemNameCheck
{
    public static string Check(string path)
    {
        string name = Path.GetFileName(path);

        bool isSystemName = File.Exists(Path.Combine(Environment.SystemDirectory, name))
                         || File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86), name));

        if (!isSystemName)
            return "No Windows system DLL has this name";

        string windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        string folder = Path.GetDirectoryName(path) ?? "";

        return LocationDescriber.IsInside(folder, windows)
            ? "Same name as a Windows system DLL, and it is inside the Windows folder"
            : "WARNING: same name as a Windows system DLL, but this file is NOT in the Windows folder";
    }
}
