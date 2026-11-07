using AppInfoGUI.Package.Inno;
using AppInfoGUI.Package.Nsis;
using System.IO;
using System.Reflection.PortableExecutable;

namespace AppInfoGUI.Package;

public record Detection(PackageFormat? Format, Overlay? Overlay);

// Recognises installers and self-extracting archives from fixed markers in the file.
// It only reads. Nothing is run or unpacked.
public static class PackageDetector
{
    const int WindowSize = 128 * 1024;          // how much of the extra data we look through

    public static Detection Detect(string path)
    {
        using FileStream stream = File.OpenRead(path);
        using var pe = new PEReader(stream);

        if (pe.PEHeaders.PEHeader == null) return new Detection(null, null);

        Overlay? overlay = Overlay.Find(pe, stream);

        // Installers with their own markers come first, so a ZIP packed inside one is not mistaken for the installer
        PackageFormat? format = FromSections(pe)
            ?? FromInnoSetup(pe, stream)
            ?? FromStart(stream, overlay);

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

    // Inno Setup keeps a table in a resource that says where its setup data starts
    static PackageFormat? FromInnoSetup(PEReader pe, Stream stream) =>
        InnoOffsetTable.Find(pe, stream) != null ? PackageFormat.InnoSetup : null;

    static byte[] Read(Stream stream, long position, int length)
    {
        var buffer = new byte[length];
        stream.Position = position;
        stream.ReadExactly(buffer, 0, length);
        return buffer;
    }
}
