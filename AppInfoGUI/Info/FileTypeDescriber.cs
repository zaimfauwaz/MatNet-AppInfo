using System.IO;

namespace AppInfoGUI.Info;

internal static class FileTypeDescriber
{
    static readonly byte[] ExecutableStart = { 0x4D, 0x5A };   // "MZ", the start of every Windows executable
    static readonly byte[] InstallerStart = { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 };   // the start of an MSI or MSP

    // Names the file type from its extension, and warns if the content does not match
    public static string Describe(string path)
    {
        string extension = Path.GetExtension(path).ToLowerInvariant();

        string name = extension switch
        {
            ".exe" => "Application",
            ".dll" => "Dynamic-link library",
            ".ocx" => "ActiveX control",
            ".sys" => "Driver",
            ".msi" => "Windows Installer package",
            ".msp" => "Windows Installer patch",
            _ => "Other file"
        };

        string result = $"{name} ({extension})";

        bool shouldBeExecutable = extension is ".exe" or ".dll" or ".ocx" or ".sys";
        bool shouldBeInstaller = extension is ".msi" or ".msp";

        if (shouldBeExecutable && !StartsWith(path, ExecutableStart))
            result += " - WARNING: content is not a Windows executable";

        if (shouldBeInstaller && !StartsWith(path, InstallerStart))
            result += " - WARNING: content is not an installer package";

        return result;
    }

    static bool StartsWith(string path, byte[] expected)
    {
        using var stream = File.OpenRead(path);
        byte[] actual = new byte[expected.Length];
        int read = stream.Read(actual, 0, actual.Length);

        return read == expected.Length && actual.AsSpan().SequenceEqual(expected);
    }
}
