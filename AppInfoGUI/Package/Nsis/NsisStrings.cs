using System.Text;

namespace AppInfoGUI.Package.Nsis;

// Reads strings from the installer's string table.
// A string may hold special codes for variables ($INSTDIR) and folders ($PROGRAMFILES).
public class NsisStrings
{
    // Codes (NSIS 3) that sit in front of a 2-byte value
    const int LanguageCode = 1;
    const int ShellFolderCode = 2;
    const int VariableCode = 3;
    const int LiteralCode = 4;      // the next character is plain text

    const int MaxLength = 1024;

    readonly byte[] _data;
    readonly int _table;
    readonly bool _unicode;

    public NsisStrings(NsisHeader header, bool unicode)
    {
        _data = header.Data;
        _table = header.StringsOffset;
        _unicode = unicode;
    }

    int UnitSize => _unicode ? 2 : 1;

    // Unicode installers hold two bytes per character. Their names look like "r\0e\0a\0".
    public static bool LooksUnicode(NsisHeader header, IEnumerable<int> nameOffsets)
    {
        var offsets = nameOffsets.Where(o => o > 0).ToList();
        if (offsets.Count == 0) return false;

        return offsets.All(o =>
        {
            int at = header.StringsOffset + o * 2;
            return at + 1 < header.Data.Length && header.Data[at] != 0 && header.Data[at + 1] == 0;
        });
    }

    public string Read(int offset)
    {
        int at = _table + offset * UnitSize;
        if (offset < 0 || at >= _data.Length) return "";

        var text = new StringBuilder();
        while (at + UnitSize <= _data.Length && text.Length < MaxLength)
        {
            int unit = Unit(ref at);
            if (unit == 0) break;

            switch (unit)
            {
                case VariableCode:
                    text.Append(NsisVariables.Name(VariableNumber(ref at)));
                    break;
                case ShellFolderCode:
                    VariableNumber(ref at);
                    text.Append("$folder");
                    break;
                case LanguageCode:
                    VariableNumber(ref at);
                    text.Append("$language");
                    break;
                case LiteralCode:
                    text.Append((char)Unit(ref at));
                    break;
                default:
                    text.Append((char)unit);
                    break;
            }
        }

        return text.ToString();
    }

    int Unit(ref int at)
    {
        if (at + UnitSize > _data.Length) return 0;

        int unit = _unicode ? BitConverter.ToUInt16(_data, at) : _data[at];
        at += UnitSize;
        return unit;
    }

    // A variable or folder number is always 2 bytes, 7 bits each, low part first
    int VariableNumber(ref int at)
    {
        if (at + 2 > _data.Length) return -1;

        int number = (_data[at] & 0x7F) | ((_data[at + 1] & 0x7F) << 7);
        at += 2;
        return number;
    }
}
