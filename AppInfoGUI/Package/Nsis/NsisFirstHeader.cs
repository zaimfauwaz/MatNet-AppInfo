using System.IO;
using System.Text;

namespace AppInfoGUI.Package.Nsis;

// The small block that marks where an NSIS installer keeps its data.
// It starts on a 512-byte boundary: flags, 0xDEADBEEF, "NullsoftInst", then two lengths.
public record NsisFirstHeader(long Position, int HeaderLength, int DataLength)
{
    public const int Size = 28;
    const int SearchWindow = 128 * 1024;   // how far into the extra data we look

    static readonly byte[] Marker = { 0xEF, 0xBE, 0xAD, 0xDE };
    static readonly byte[] Name = Encoding.ASCII.GetBytes("NullsoftInst");

    // Where the packed data begins
    public long DataStart => Position + Size;

    // Null when the file is not an NSIS installer
    public static NsisFirstHeader? Find(Stream file, Overlay? overlay)
    {
        if (overlay == null) return null;

        var window = new byte[(int)Math.Min(overlay.Length, SearchWindow)];
        file.Position = overlay.Start;
        file.ReadExactly(window, 0, window.Length);

        long firstBoundary = (overlay.Start + 511) / 512 * 512;
        for (long at = firstBoundary; at - overlay.Start + Size <= window.Length; at += 512)
        {
            var block = window.AsSpan((int)(at - overlay.Start), Size);
            if (block.Slice(4, 4).SequenceEqual(Marker) && block.Slice(8, 12).SequenceEqual(Name))
                return new NsisFirstHeader(at, BitConverter.ToInt32(block.Slice(20, 4)), BitConverter.ToInt32(block.Slice(24, 4)));
        }

        return null;
    }
}
