using System.IO;

namespace AppInfoGUI.Package.Nsis;

// The installer's script header: a table of "instructions" and a table of strings.
// Each instruction is 7 numbers: what to do, then up to 6 arguments.
public class NsisHeader
{
    const int BlockTable = 4;       // the header starts with a flags number, then 8 blocks of (offset, count)
    const int EntriesBlock = 2;
    const int StringsBlock = 3;
    const int EntrySize = 28;

    readonly byte[] _data;

    public int EntryCount { get; }
    public int StringsOffset { get; }
    int EntriesOffset { get; }

    public NsisHeader(byte[] data)
    {
        _data = data;
        if (data.Length < BlockTable + 8 * 8) throw new InvalidDataException("the NSIS header is too short");

        EntriesOffset = Number(BlockTable + EntriesBlock * 8);
        EntryCount = Number(BlockTable + EntriesBlock * 8 + 4);
        StringsOffset = Number(BlockTable + StringsBlock * 8);

        bool inside = EntriesOffset > 0 && EntryCount >= 0
                      && EntriesOffset + (long)EntryCount * EntrySize <= data.Length
                      && StringsOffset > 0 && StringsOffset < data.Length;
        if (!inside) throw new InvalidDataException("the NSIS header is damaged");
    }

    public byte[] Data => _data;

    // What instruction number `index` does
    public int Opcode(int index) => Number(EntriesOffset + index * EntrySize);

    // Argument 0 to 5 of instruction number `index`
    public int Argument(int index, int argument) => Number(EntriesOffset + index * EntrySize + 4 + argument * 4);

    int Number(int at) => BitConverter.ToInt32(_data, at);
}
