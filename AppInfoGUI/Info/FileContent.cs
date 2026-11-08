using System.IO;

namespace AppInfoGUI.Info;

public enum ContentType { Unknown, Program, OleFile, Zip, SevenZip, Rar, Cabinet, Pdf, Picture, Gzip, Text }

public record ContentInfo(ContentType Type, string Name);

// What a file really is, judged from its first bytes. A name can say anything, the first bytes cannot.
public static class FileContent
{
    const int Window = 512;

    static readonly (byte[] Start, ContentInfo Info)[] Known =
    {
        (new byte[] { 0x4D, 0x5A }, new(ContentType.Program, "Windows program or library")),
        (new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 }, new(ContentType.OleFile, "Windows Installer package or old Office file")),
        (new byte[] { 0x50, 0x4B, 0x03, 0x04 }, new(ContentType.Zip, "ZIP archive")),
        (new byte[] { 0x37, 0x7A, 0xBC, 0xAF, 0x27, 0x1C }, new(ContentType.SevenZip, "7-Zip archive")),
        (new byte[] { 0x52, 0x61, 0x72, 0x21, 0x1A, 0x07 }, new(ContentType.Rar, "RAR archive")),
        (new byte[] { 0x4D, 0x53, 0x43, 0x46 }, new(ContentType.Cabinet, "Cabinet (CAB) archive")),
        (new byte[] { 0x25, 0x50, 0x44, 0x46 }, new(ContentType.Pdf, "PDF document")),
        (new byte[] { 0x89, 0x50, 0x4E, 0x47 }, new(ContentType.Picture, "PNG picture")),
        (new byte[] { 0xFF, 0xD8, 0xFF }, new(ContentType.Picture, "JPEG picture")),
        (new byte[] { 0x47, 0x49, 0x46, 0x38 }, new(ContentType.Picture, "GIF picture")),
        (new byte[] { 0x1F, 0x8B }, new(ContentType.Gzip, "Gzip archive"))
    };

    // Throws if the file cannot be read, so the caller can say "could not check" instead of guessing
    public static ContentInfo Detect(string path)
    {
        using FileStream stream = File.OpenRead(path);
        var buffer = new byte[Window];
        int read = stream.ReadAtLeast(buffer, Window, throwOnEndOfStream: false);

        return Identify(buffer.AsSpan(0, read));
    }

    static ContentInfo Identify(ReadOnlySpan<byte> start)
    {
        if (start.Length == 0)
            return new(ContentType.Unknown, "Empty file");

        foreach ((byte[] mark, ContentInfo info) in Known)
            if (start.StartsWith(mark))
                return info;

        return LooksLikeText(start) ? new(ContentType.Text, "Text file or script") : new(ContentType.Unknown, "Unrecognised data");
    }

    // Text has no zero bytes and no control characters other than tab and line breaks
    static bool LooksLikeText(ReadOnlySpan<byte> start)
    {
        foreach (byte b in start)
            if (b < 0x20 && b is not (9 or 10 or 13))
                return false;

        return true;
    }
}
