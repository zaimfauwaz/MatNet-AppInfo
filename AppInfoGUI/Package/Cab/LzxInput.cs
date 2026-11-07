using System.IO;

namespace AppInfoGUI.Package.Cab;

// The packed bytes of one LZX block of a cabinet. The bits are read in 16-bit little-endian words,
// from the high end of each word.
internal class LzxInput : IBitSource
{
    readonly byte[] _data;
    ulong _buffer;
    int _count;     // how many bits of the buffer are still unread

    public int Position { get; private set; }

    public LzxInput(byte[] data) => _data = data;

    public int ReadBits(int count)
    {
        if (count == 0) return 0;

        while (_count < count)
        {
            int word = Position + 1 < _data.Length ? _data[Position] | (_data[Position + 1] << 8) : 0;
            Position += 2;
            _buffer = (_buffer << 16) | (uint)word;
            _count += 16;
        }

        _count -= count;
        return (int)((_buffer >> _count) & ((1UL << count) - 1));
    }

    public int ReadBit() => ReadBits(1);

    // Forgets the bits that are left, to go on with whole bytes. A word that was not started is skipped.
    public void AlignToBytes()
    {
        if (_count == 0) Position += 2;
        _buffer = 0;
        _count = 0;
    }

    public byte[] ReadBytes(int count)
    {
        if (Position + count > _data.Length) throw new InvalidDataException("the packed data ended too early");

        var bytes = new byte[count];
        Array.Copy(_data, Position, bytes, 0, count);
        Position += count;
        return bytes;
    }

    // Used after the bytes were taken straight from the data
    public void SkipBytes(int count) => Position += count;
}
