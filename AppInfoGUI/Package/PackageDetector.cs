using AppInfoGUI.Info;
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

    static readonly byte[] SevenZipMark = { 0x37, 0x7A, 0xBC, 0xAF, 0x27, 0x1C };
    static readonly byte[] RarMark = { 0x52, 0x61, 0x72, 0x21, 0x1A, 0x07 };               // "Rar!" 1A 07
    static readonly byte[] ZipMark = { 0x50, 0x4B, 0x03, 0x04 };                           // "PK" 03 04
    static readonly byte[] PyInstallerMark = Encoding.ASCII.GetBytes("MEI").Concat(new byte[] { 0x0C, 0x0B, 0x0A, 0x0B, 0x0E }).ToArray();

    public static Detection Detect(string path)
    {
        using FileStream stream = File.OpenRead(path);
        using var pe = new PEReader(stream);

        if (pe.PEHeaders.PEHeader == null) return new Detection(null, null);

        Overlay? overlay = Overlay.Find(pe, stream);

        PackageFormat? format = FromSections(pe)
            ?? FromStart(stream, overlay)
            ?? FromEnd(stream, overlay)
            ?? FromInnoResource(path)
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

        byte[] window = Read(stream, overlay.Start, (int)Math.Min(overlay.Length, WindowSize));
        if (FromNsis(window, overlay.Start)) return PackageFormat.Nsis;

        // Whichever mark comes first is the archive
        var found = new (int Index, PackageFormat Format)[]
        {
            (window.AsSpan().IndexOf(SevenZipMark), PackageFormat.SevenZip),
            (window.AsSpan().IndexOf(RarMark), PackageFormat.Rar),
            (window.AsSpan().IndexOf(ZipMark), PackageFormat.Zip)
        };

        var first = found.Where(f => f.Index >= 0).OrderBy(f => f.Index).ToList();
        return first.Count > 0 ? first[0].Format : null;
    }

    // NSIS starts its data on a 512-byte boundary with 0xDEADBEEF followed by "NullsoftInst"
    static bool FromNsis(byte[] window, long windowStart)
    {
        byte[] marker = { 0xEF, 0xBE, 0xAD, 0xDE };
        byte[] name = Encoding.ASCII.GetBytes("NullsoftInst");

        long firstBoundary = (windowStart + 511) / 512 * 512;
        for (long at = firstBoundary; at - windowStart + 20 <= window.Length; at += 512)
        {
            var header = window.AsSpan((int)(at - windowStart), 20);
            if (header.Slice(4, 4).SequenceEqual(marker) && header.Slice(8, 12).SequenceEqual(name))
                return true;
        }

        return false;
    }

    // PyInstaller writes a marker at the very end of its data
    static PackageFormat? FromEnd(Stream stream, Overlay? overlay)
    {
        if (overlay == null) return null;

        int length = (int)Math.Min(overlay.Length, WindowSize);
        byte[] tail = Read(stream, overlay.End - length, length);
        return tail.AsSpan().IndexOf(PyInstallerMark) >= 0 ? PackageFormat.PyInstaller : null;
    }

    // Inno Setup keeps its loader table in a resource numbered 11111
    static PackageFormat? FromInnoResource(string path)
    {
        try
        {
            IntPtr module = IconNative.LoadLibraryEx(path, IntPtr.Zero, IconNative.LoadAsDataFile);
            if (module == IntPtr.Zero) return null;

            try
            {
                IntPtr rcData = (IntPtr)10;
                bool found = IconNative.FindResource(module, (IntPtr)11111, rcData) != IntPtr.Zero;
                return found ? PackageFormat.InnoSetup : null;
            }
            finally
            {
                IconNative.FreeLibrary(module);
            }
        }
        catch (Exception)
        {
            return null;   // not on Windows, or the resources are unreadable
        }
    }

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
