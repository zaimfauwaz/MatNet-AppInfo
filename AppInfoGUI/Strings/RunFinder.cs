using System.Text;

namespace AppInfoGUI.Strings;

// Finds runs of readable characters in bytes fed to it in pieces. Works for plain text (1 byte per character)
// and for Windows "wide" text (2 bytes per character, the second one zero).
internal class RunFinder
{
    const int MaxLength = 2000;     // a longer run is split, so one huge blob cannot use all the memory

    readonly bool _wide;
    readonly int _minLength;
    readonly Action<string, long> _found;
    readonly StringBuilder _text = new();

    long _start;
    int _pending = -1;              // wide text only: a character waiting for its zero byte
    long _pendingAt;

    public RunFinder(bool wide, int minLength, Action<string, long> found)
    {
        _wide = wide;
        _minLength = minLength;
        _found = found;
    }

    public void Feed(ReadOnlySpan<byte> bytes, long offset)
    {
        for (int i = 0; i < bytes.Length; i++)
            Add(bytes[i], offset + i);
    }

    public void End() => Flush();

    void Add(byte b, long at)
    {
        if (!_wide)
        {
            AddPlain(b, at);
            return;
        }

        if (_pending >= 0 && b == 0)
        {
            Append((char)_pending, _pendingAt);
            _pending = -1;
            return;
        }

        if (_pending >= 0 || !IsReadable(b))
            Flush();                // the run is broken

        _pending = IsReadable(b) ? b : -1;
        _pendingAt = at;
    }

    void AddPlain(byte b, long at)
    {
        if (IsReadable(b))
            Append((char)b, at);
        else
            Flush();
    }

    void Append(char c, long at)
    {
        if (_text.Length == 0)
            _start = at;

        _text.Append(c);
        if (_text.Length >= MaxLength)
            Flush();
    }

    void Flush()
    {
        if (_text.Length >= _minLength)
            _found(_text.ToString(), _start);

        _text.Clear();
    }

    static bool IsReadable(byte b) => b is >= 0x20 and <= 0x7E || b == 9;
}
