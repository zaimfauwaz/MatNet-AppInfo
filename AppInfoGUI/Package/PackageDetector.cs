using AppInfoGUI.Package.Inno;
using AppInfoGUI.Package.Nsis;
using System.Diagnostics;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Text;

namespace AppInfoGUI.Package;

public record Detection(PackageFormat? Format, Overlay? Overlay);

// Recognises installers and self-extracting archives from fixed markers in the file.
// It only reads. Nothing is run or unpacked.
public static class PackageDetector
{
    const int WindowSize = 128 * 1024;          // how much of the extra data we look through
    const long UnknownDataLimit = 1024 * 1024;  // extra data below this size is ignored

    static readonly byte[] PyInstallerMark = Encoding.ASCII.GetBytes("MEI").Concat(new byte[] { 0x0C, 0x0B, 0x0A, 0x0B, 0x0E }).ToArray();

    public static Detection Detect(string path)
    {
        using FileStream stream = File.OpenRead(path);
        using var pe = new PEReader(stream);

        if (pe.PEHeaders.PEHeader == null) return new Detection(null, null);

        Overlay? overlay = Overlay.Find(pe, stream);

        // Installers with their own markers come first, so a ZIP packed inside one is not mistaken for the installer
        PackageFormat? format = FromSections(pe)
            ?? FromInnoSetup(pe, stream)
            ?? FromStart(stream, overlay)
            ?? FromEnd(stream, overlay)
            ?? FromVersionInfo(path);

        if (format == null && overlay != null && overlay.Length >= UnknownDataLimit)
            format = PackageFormat.UnknownData;

        return new Detection(format, overlay);
    }

    // WiX bundles have a section with its own name
    static PackageFormat? FromSections(PEReader pe) =>
        pe.PEHeaders.SectionHeaders.Any(s => s.Name == ".wixburn") ? PackageFormat.WixBurn : null;

    // Archives and NSIS data sit at the start of the extra data, sometimes after a short settings block
    static PackageFormat? FromStart(Stream stream, Overlay? overlay)
    {
        if (overlay == null) return null;

        if (NsisFirstHeader.Find(stream, overlay) != null) return PackageFormat.Nsis;

        byte[] window = Read(stream, overlay.Start, (int)Math.Min(overlay.Length, WindowSize));

        // Whichever mark comes first is the archive
        var found = new (int Index, PackageFormat Format)[]
        {
            (window.AsSpan().IndexOf(Signatures.SevenZip), PackageFormat.SevenZip),
            (window.AsSpan().IndexOf(Signatures.Rar), PackageFormat.Rar),
            (window.AsSpan().IndexOf(Signatures.Zip), PackageFormat.Zip)
        };

        var first = found.Where(f => f.Index >= 0).OrderBy(f => f.Index).ToList();
        return first.Count > 0 ? first[0].Format : null;
    }

    // PyInstaller writes a marker at the very end of its data
    static PackageFormat? FromEnd(Stream stream, Overlay? overlay)
    {
        if (overlay == null) return null;

        int length = (int)Math.Min(overlay.Length, WindowSize);
        byte[] tail = Read(stream, overlay.End - length, length);
        return tail.AsSpan().IndexOf(PyInstallerMark) >= 0 ? PackageFormat.PyInstaller : null;
    }

    // Inno Setup keeps a table in a resource that says where its setup data starts
    static PackageFormat? FromInnoSetup(PEReader pe, Stream stream) =>
        InnoOffsetTable.Find(pe, stream) != null ? PackageFormat.InnoSetup : null;

    // InstallShield names itself in the version information
    static PackageFormat? FromVersionInfo(string path)
    {
        try
        {
            FileVersionInfo info = FileVersionInfo.GetVersionInfo(path);
            string text = $"{info.CompanyName} {info.ProductName} {info.FileDescription}";
            return text.Contains("InstallShield", StringComparison.OrdinalIgnoreCase) ? PackageFormat.InstallShield : null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    static byte[] Read(Stream stream, long position, int length)
    {
        var buffer = new byte[length];
        stream.Position = position;
        stream.ReadExactly(buffer, 0, length);
        return buffer;
    }
}
