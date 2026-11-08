using System.IO;

namespace AppInfoGUI.Package.Cab;

// MSZIP: every block is "CK" and then deflate data. A block may point back into the blocks before it.
internal class MsZipDecoder : ICabDecoder
{
    static readonly int[] LengthBase = { 3, 4, 5, 6, 7, 8, 9, 10, 11, 13, 15, 17, 19, 23, 27, 31, 35, 43, 51, 59, 67, 83, 99, 115, 131, 163, 195, 227, 258 };
    static readonly int[] LengthExtra = { 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5, 0 };
    static readonly int[] DistanceBase = { 1, 2, 3, 4, 5, 7, 9, 13, 17, 25, 33, 49, 65, 97, 129, 193, 257, 385, 513, 769, 1025, 1537, 2049, 3073, 4097, 6145, 8193, 12289, 16385, 24577 };
    static readonly int[] DistanceExtra = { 0, 0, 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 8, 9, 9, 10, 10, 11, 11, 12, 12, 13, 13 };
    static readonly int[] CodeLengthOrder = { 16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15 };

    const int MaxBits = 15;
    const string Damaged = "the packed data is damaged";

    public void Decode(byte[] block, int size, GrowingBuffer output)
    {
        if (block.Length < 2 || block[0] != 'C' || block[1] != 'K') throw new InvalidDataException(Damaged);

        int start = output.Length;
        var bits = new DeflateBits(block, 2);

        bool last;
        do
        {
            last = bits.Read(1) == 1;
            switch (bits.Read(2))
            {
                case 0: Stored(bits, output); break;
                case 1: Compressed(bits, output, FixedLiterals(), FixedDistances()); break;
                case 2: Dynamic(bits, output); break;
                default: throw new InvalidDataException(Damaged);
            }
        } while (!last);

        if (output.Length - start != size) throw new InvalidDataException(Damaged);
    }

    static void Stored(DeflateBits bits, GrowingBuffer output)
    {
        bits.AlignToByte();
        int length = bits.Read(16);
        bits.Read(16);   // the length again, inverted

        for (int i = 0; i < length; i++) output.Append((byte)bits.Read(8));
    }

    static void Dynamic(DeflateBits bits, GrowingBuffer output)
    {
        int literalCount = bits.Read(5) + 257;
        int distanceCount = bits.Read(5) + 1;
        int codeLengthCount = bits.Read(4) + 4;
        if (literalCount > 286 || distanceCount > 30) throw new InvalidDataException(Damaged);

        // The lengths of the codes are themselves packed with a small code
        var lengthCodeLengths = new int[19];
        for (int i = 0; i < codeLengthCount; i++) lengthCodeLengths[CodeLengthOrder[i]] = bits.Read(3);
        var lengthCode = new CanonicalHuffman(lengthCodeLengths, 0, 19, 7);

        int[] lengths = ReadCodeLengths(bits, lengthCode, literalCount + distanceCount);

        Compressed(bits, output,
            new CanonicalHuffman(lengths, 0, literalCount, MaxBits),
            new CanonicalHuffman(lengths, literalCount, distanceCount, MaxBits));
    }

    // Symbols 0-15 are a length as is. 16 repeats the previous length, 17 and 18 repeat zero.
    static int[] ReadCodeLengths(DeflateBits bits, CanonicalHuffman lengthCode, int total)
    {
        var lengths = new int[total];
        int i = 0;
        while (i < total)
        {
            int symbol = lengthCode.Decode(bits);
            if (symbol < 16)
            {
                lengths[i++] = symbol;
                continue;
            }

            int value = 0;
            int repeat;
            if (symbol == 16)
            {
                if (i == 0) throw new InvalidDataException(Damaged);
                value = lengths[i - 1];
                repeat = 3 + bits.Read(2);
            }
            else if (symbol == 17) repeat = 3 + bits.Read(3);
            else repeat = 11 + bits.Read(7);

            if (i + repeat > total) throw new InvalidDataException(Damaged);
            for (int n = 0; n < repeat; n++) lengths[i++] = value;
        }

        return lengths;
    }

    static void Compressed(DeflateBits bits, GrowingBuffer output, CanonicalHuffman literals, CanonicalHuffman distances)
    {
        while (true)
        {
            int symbol = literals.Decode(bits);
            if (symbol < 256) { output.Append((byte)symbol); continue; }
            if (symbol == 256) return;

            symbol -= 257;
            if (symbol >= LengthBase.Length) throw new InvalidDataException(Damaged);
            int length = LengthBase[symbol] + bits.Read(LengthExtra[symbol]);

            int slot = distances.Decode(bits);
            if (slot >= DistanceBase.Length) throw new InvalidDataException(Damaged);
            int distance = DistanceBase[slot] + bits.Read(DistanceExtra[slot]);

            output.CopyMatch(distance, length);
        }
    }

    static CanonicalHuffman FixedLiterals()
    {
        var lengths = new int[288];
        for (int i = 0; i < 288; i++) lengths[i] = FixedLiteralLength(i);
        return new CanonicalHuffman(lengths, 0, 288, MaxBits);
    }

    // The fixed code: 0-143 use 8 bits, 144-255 use 9, 256-279 use 7, 280-287 use 8
    static int FixedLiteralLength(int symbol)
    {
        if (symbol < 144) return 8;
        if (symbol < 256) return 9;
        if (symbol < 280) return 7;
        return 8;
    }

    static CanonicalHuffman FixedDistances()
    {
        var lengths = new int[30];
        Array.Fill(lengths, 5);
        return new CanonicalHuffman(lengths, 0, 30, MaxBits);
    }
}

// Deflate reads its bits starting from the low end of each byte
internal class DeflateBits : IBitSource
{
    readonly byte[] _data;
    int _position;
    int _bit;    // which bit of the current byte is next

    public DeflateBits(byte[] data, int start)
    {
        _data = data;
        _position = start;
    }

    public int ReadBit()
    {
        if (_position >= _data.Length) throw new InvalidDataException("the packed data ended too early");

        int value = (_data[_position] >> _bit) & 1;
        if (++_bit == 8) { _bit = 0; _position++; }
        return value;
    }

    public int Read(int count)
    {
        int value = 0;
        for (int i = 0; i < count; i++) value |= ReadBit() << i;
        return value;
    }

    public void AlignToByte()
    {
        if (_bit == 0) return;
        _bit = 0;
        _position++;
    }
}
