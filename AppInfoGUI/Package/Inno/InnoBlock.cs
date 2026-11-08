using SharpCompress.Compressors.LZMA;
using System.IO;

namespace AppInfoGUI.Package.Inno;

// A packed block of the setup data. It is stored as a small header (checksum, size, "is packed"),
// then the data in chunks of 4096 bytes, each with its own 4-byte checksum.
public sealed class InnoBlock : IDisposable
{
    const int ChunkSize = 4096;
    const long MaxStoredSize = 64 * 1024 * 1024;

    // The unpacked data. Nothing is written anywhere.
    public Stream Data { get; }

    // Where the next block starts
    public long End { get; }

    InnoBlock(Stream data, long end)
    {
        Data = data;
        End = end;
    }

    // `wideSize`: the size of the block is written in 8 bytes (Inno Setup 6.7 and later) instead of 4
    public static InnoBlock Open(Stream file, long start, bool wideSize)
    {
        int headerSize = wideSize ? 13 : 9;
        var head = new byte[headerSize];
        file.Position = start;
        file.ReadExactly(head, 0, headerSize);

        long stored = wideSize ? BitConverter.ToInt64(head, 4) : BitConverter.ToUInt32(head, 4);
        bool packed = head[headerSize - 1] != 0;
        if (stored < 0 || stored > MaxStoredSize || start + headerSize + stored > file.Length)
            throw new InvalidDataException("the Inno Setup data is damaged");

        var raw = new byte[(int)stored];
        file.ReadExactly(raw, 0, raw.Length);

        var chunks = new MemoryStream();
        for (int i = 0; i + 4 < raw.Length;)
        {
            int length = Math.Min(ChunkSize, raw.Length - i - 4);
            chunks.Write(raw, i + 4, length);
            i += 4 + length;
        }

        chunks.Position = 0;
        return new InnoBlock(packed ? Unpack(chunks) : chunks, start + headerSize + stored);
    }

    // LZMA: 5 bytes of settings, then the data
    static Stream Unpack(Stream chunks)
    {
        var settings = new byte[5];
        chunks.ReadExactly(settings, 0, 5);
        return LzmaStream.Create(settings, chunks, -1, -1);
    }

    public void Dispose() => Data.Dispose();
}
