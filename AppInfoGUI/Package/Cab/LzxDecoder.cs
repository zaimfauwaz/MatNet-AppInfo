using System.IO;

namespace AppInfoGUI.Package.Cab;

// LZX, as used in cabinets. Every cabinet block is one 32 KB frame, but the state (trees, recent offsets)
// carries over from frame to frame. Matches point into the raw data, so that is kept in a window of its own.
// The "E8" filter that makes program code pack better is undone on the copy that is handed out.
internal class LzxDecoder : ICabDecoder
{
    const int BlockVerbatim = 1;
    const int BlockAligned = 2;
    const int BlockUncompressed = 3;

    const int MinMatch = 2;
    const int SecondaryLengths = 249;
    const int PretreeSymbols = 20;
    const int MaxBits = 16;

    static readonly int[] ExtraBits = new int[52];
    static readonly int[] PositionBase = new int[52];

    static LzxDecoder()
    {
        for (int i = 0, j = 0; i <= 50; i += 2)
        {
            ExtraBits[i] = ExtraBits[i + 1] = j;
            if (i != 0 && j < 17) j++;
        }

        for (int i = 0, j = 0; i <= 50; i++)
        {
            PositionBase[i] = j;
            j += 1 << ExtraBits[i];
        }
    }

    readonly int _mainSymbols;
    readonly int[] _mainLengths;
    readonly int[] _lengthLengths = new int[SecondaryLengths];

    CanonicalHuffman? _main;
    CanonicalHuffman? _length;
    CanonicalHuffman? _aligned;

    readonly GrowingBuffer _window = new();    // the raw data, which matches point into

    bool _started;
    uint _intelFileSize;     // the size the "E8" filter works with, or 0 when it is not used
    bool _intelStarted;      // the filter only applies once a block could hold program code
    int _blockType;
    int _blockLength;
    int _blockRemaining;
    int _r0 = 1, _r1 = 1, _r2 = 1;    // the three most recent match distances

    public LzxDecoder(int windowBits)
    {
        if (windowBits < 15 || windowBits > 21) throw new NotSupportedException("this LZX window size is not supported");

        int slots = windowBits switch { 15 => 30, 16 => 32, 17 => 34, 18 => 36, 19 => 38, 20 => 42, _ => 50 };
        _mainSymbols = 256 + slots * 8;
        _mainLengths = new int[_mainSymbols];
    }

    public void Decode(byte[] block, int size, GrowingBuffer output)
    {
        var input = new LzxInput(block);
        int frameStart = _window.Length;
        int target = frameStart + size;

        if (!_started)
        {
            // A flag, and then the size the "E8" filter works with
            if (input.ReadBits(1) == 1) _intelFileSize = (uint)input.ReadBits(32);
            _started = true;
        }

        while (_window.Length < target)
        {
            if (_blockRemaining == 0) StartBlock(input);

            int run = Math.Min(_blockRemaining, target - _window.Length);
            if (_blockType == BlockUncompressed)
            {
                _window.Append(input.ReadBytes(run), 0, run);

                // An uncompressed block with an odd length is followed by one byte of padding
                if (run == _blockRemaining && (_blockLength & 1) == 1) input.SkipBytes(1);
            }
            else
            {
                DecodeRun(input, run);
            }

            _blockRemaining -= run;
        }

        byte[] frame = _window.ToArray(frameStart, size);
        if (_intelStarted && _intelFileSize != 0 && frameStart < 0x40000000) UndoFilter(frame, frameStart);
        output.Append(frame, 0, frame.Length);
    }

    // The filter turned the target of each "call" instruction (E8) into an absolute address. This puts it back.
    void UndoFilter(byte[] frame, int frameStart)
    {
        int i = 0;
        while (i < frame.Length - 10)
        {
            if (frame[i] != 0xE8)
            {
                i++;
                continue;
            }

            int position = frameStart + i;
            int absolute = BitConverter.ToInt32(frame, i + 1);
            if (absolute >= -position && absolute < _intelFileSize)
            {
                int relative = absolute >= 0 ? absolute - position : absolute + (int)_intelFileSize;
                BitConverter.TryWriteBytes(frame.AsSpan(i + 1, 4), relative);
            }

            i += 5;   // the E8 byte and its 4-byte address
        }
    }

    void StartBlock(LzxInput input)
    {
        _blockType = input.ReadBits(3);
        if (_blockType == BlockUncompressed) _intelStarted = true;
        _blockLength = input.ReadBits(24);
        _blockRemaining = _blockLength;

        switch (_blockType)
        {
            case BlockAligned:
                var alignedLengths = new int[8];
                for (int i = 0; i < 8; i++) alignedLengths[i] = input.ReadBits(3);
                _aligned = new CanonicalHuffman(alignedLengths, 0, 8, MaxBits);
                ReadTrees(input);
                break;

            case BlockVerbatim:
                ReadTrees(input);
                break;

            case BlockUncompressed:
                input.AlignToBytes();
                _r0 = BitConverter.ToInt32(input.ReadBytes(4));
                _r1 = BitConverter.ToInt32(input.ReadBytes(4));
                _r2 = BitConverter.ToInt32(input.ReadBytes(4));
                break;

            default:
                throw new InvalidDataException("the packed data is damaged");
        }
    }

    // The lengths of the codes are stored as changes from the lengths of the block before
    void ReadTrees(LzxInput input)
    {
        ReadLengths(input, _mainLengths, 0, 256);
        ReadLengths(input, _mainLengths, 256, _mainSymbols);
        _main = new CanonicalHuffman(_mainLengths, 0, _mainSymbols, MaxBits);
        if (_mainLengths[0xE8] != 0) _intelStarted = true;

        ReadLengths(input, _lengthLengths, 0, SecondaryLengths);
        _length = new CanonicalHuffman(_lengthLengths, 0, SecondaryLengths, MaxBits);
    }

    static void ReadLengths(LzxInput input, int[] lengths, int first, int last)
    {
        var pretreeLengths = new int[PretreeSymbols];
        for (int i = 0; i < PretreeSymbols; i++) pretreeLengths[i] = input.ReadBits(4);
        var pretree = new CanonicalHuffman(pretreeLengths, 0, PretreeSymbols, MaxBits);

        for (int x = first; x < last;)
        {
            int symbol = pretree.Decode(input);

            if (symbol == 17 || symbol == 18)
            {
                int zeros = symbol == 17 ? input.ReadBits(4) + 4 : input.ReadBits(5) + 20;
                Fill(lengths, ref x, last, zeros, 0);
            }
            else if (symbol == 19)
            {
                int repeat = input.ReadBits(1) + 4;
                Fill(lengths, ref x, last, repeat, Delta(lengths[x], pretree.Decode(input)));
            }
            else
            {
                lengths[x] = Delta(lengths[x], symbol);
                x++;
            }
        }
    }

    static int Delta(int before, int change)
    {
        int value = before - change;
        return value < 0 ? value + 17 : value;
    }

    static void Fill(int[] lengths, ref int x, int last, int count, int value)
    {
        if (x + count > last) throw new InvalidDataException("the packed data is damaged");
        while (count-- > 0) lengths[x++] = value;
    }

    void DecodeRun(LzxInput input, int run)
    {
        int end = _window.Length + run;

        while (_window.Length < end)
        {
            int symbol = _main!.Decode(input);
            if (symbol < 256)
            {
                _window.Append((byte)symbol);
                continue;
            }

            symbol -= 256;
            int length = symbol & 7;
            if (length == 7) length += _length!.Decode(input);
            length += MinMatch;

            int distance = ReadDistance(input, symbol >> 3);
            if (_window.Length + length > end) throw new InvalidDataException("the packed data is damaged");
            _window.CopyMatch(distance, length);
        }
    }

    // The first three slots reuse recent distances. The others are new ones.
    int ReadDistance(LzxInput input, int slot)
    {
        switch (slot)
        {
            case 0:
                return _r0;
            case 1:
                (_r0, _r1) = (_r1, _r0);
                return _r0;
            case 2:
                (_r0, _r2) = (_r2, _r0);
                return _r0;
        }

        int extra = ExtraBits[slot];
        int bits;
        if (_blockType == BlockAligned && extra >= 3)
        {
            // The last three bits of the distance have a code of their own
            bits = input.ReadBits(extra - 3) << 3;
            bits += _aligned!.Decode(input);
        }
        else
        {
            bits = input.ReadBits(extra);
        }

        int distance = PositionBase[slot] - 2 + bits;
        _r2 = _r1;
        _r1 = _r0;
        _r0 = distance;
        return distance;
    }
}
