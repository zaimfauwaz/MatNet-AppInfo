using SharpCompress.Compressors.LZMA;
using System.IO;
using System.IO.Compression;

namespace AppInfoGUI.Package.Nsis;

public enum NsisMethod { Lzma, Bzip2, Deflate }

// NSIS compresses with zlib (raw deflate), bzip2 or LZMA. The first bytes show which.
public static class NsisCompression
{
    // LZMA and bzip2 streams announce themselves. Deflate does not, so null means "no announcement".
    public static NsisMethod? Announced(ReadOnlySpan<byte> start)
    {
        // LZMA: a properties byte (always 0x5D in NSIS) and a 4-byte dictionary size
        if (start[0] == 0x5D && BitConverter.ToUInt32(start.Slice(1, 4)) <= 1u << 30) return NsisMethod.Lzma;

        // bzip2: the character '1' and a block size
        if (start[0] == 0x31 && start[1] < 14) return NsisMethod.Bzip2;

        return null;
    }

    // Opens a stream that gives the unpacked bytes. Nothing is written anywhere.
    public static Stream Open(Stream packed)
    {
        var start = new byte[5];
        packed.ReadExactly(start, 0, 5);

        switch (Announced(start) ?? NsisMethod.Deflate)
        {
            case NsisMethod.Lzma:
                return LzmaStream.Create(start, packed, -1, -1);

            case NsisMethod.Bzip2:
                throw new NotSupportedException("the installer is compressed with bzip2, which is not supported yet");

            default:
                packed.Position = 0;
                return new DeflateStream(packed, CompressionMode.Decompress);
        }
    }
}
