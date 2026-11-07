using System.IO;

namespace AppInfoGUI.Package.Cab;

// Something that gives the packed data one bit at a time
internal interface IBitSource
{
    int ReadBit();
}

// A Huffman code described by the length of the code of each symbol. Used by deflate (MSZIP) and LZX.
// Codes of the same length are given out in the order of their symbols.
internal class CanonicalHuffman
{
    readonly int[] _count;     // how many codes there are of each length
    readonly int[] _symbol;    // the symbols, ordered by code length and then by value
    readonly int _maxLength;

    public CanonicalHuffman(int[] lengths, int first, int count, int maxLength)
    {
        _maxLength = maxLength;
        _count = new int[maxLength + 1];
        _symbol = new int[count];

        for (int i = 0; i < count; i++) _count[lengths[first + i]]++;

        var offsets = new int[maxLength + 2];
        for (int length = 1; length <= maxLength; length++) offsets[length + 1] = offsets[length] + _count[length];

        for (int i = 0; i < count; i++)
        {
            int length = lengths[first + i];
            if (length != 0) _symbol[offsets[length]++] = i;
        }
    }

    public int Decode(IBitSource bits)
    {
        int code = 0, first = 0, index = 0;

        for (int length = 1; length <= _maxLength; length++)
        {
            code |= bits.ReadBit();
            int count = _count[length];
            if (code - count < first) return _symbol[index + (code - first)];

            index += count;
            first += count;
            first <<= 1;
            code <<= 1;
        }

        throw new InvalidDataException("the packed data is damaged");
    }
}
