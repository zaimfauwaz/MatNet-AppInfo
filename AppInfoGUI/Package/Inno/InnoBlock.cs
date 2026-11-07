using SharpCompress.Compressors.LZMA;
using System.IO;

namespace AppInfoGUI.Package.Inno;

// A packed block of the setup data. It is stored as a small header (checksum, size, "is packed"),
// then the data in chunks of 4096 bytes, each with its own 4-byte checksum.
public class InnoBlock : IDisposable
{
    const int ChunkSize = 4096;
    const uint MaxStoredSize = 64 * 1024 * 1024;

    // The unpacked data. Nothing is written anywhere.
    public Stream Data { get; }

    // Where the next block starts
    public long End { get; }

    InnoBlock(Stream data, long end)
    {
        Data = data;
        End = end;
    }

    public static InnoBlock Open(Stream file, long start)
    {
        var head = new byte[9];
        file.Position = start;
        file.ReadExactly(head, 0, 9);

        uint stored = BitConverter.ToUInt32(head, 4);
        bool packed = head[8] != 0;
        if (stored > MaxStoredSize || start + 9 + stored > file.Length)
            throw new InvalidDataException("the Inno Setup data is damaged");

        var raw = new byte[stored];
        file.ReadExactly(raw, 0, raw.Length);

        var chunks = new MemoryStream();
        for (int i = 0; i + 4 < raw.Length;)
        {
            int length = Math.Min(ChunkSize, raw.Length - i - 4);
            chunks.Write(raw, i + 4, length);
            i += 4 + length;
        }

        chunks.Position = 0;
        return new InnoBlock(packed ? Unpack(chunks) : chunks, start + 9 + stored);
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
