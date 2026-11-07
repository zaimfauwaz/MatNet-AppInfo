using System.IO;
using System.Text;

namespace AppInfoGUI.Package.Inno;

// Reads the pieces the setup data is made of: numbers, strings and fixed blocks.
public class InnoReader
{
    const int MaxStringBytes = 16 * 1024 * 1024;

    readonly Stream _stream;

    public InnoReader(Stream stream) => _stream = stream;

    public byte[] Bytes(int count)
    {
        var bytes = new byte[count];
        _stream.ReadExactly(bytes, 0, count);
        return bytes;
    }

    public int Int32() => BitConverter.ToInt32(Bytes(4));

    // A length in bytes, then UTF-16 text
    public string Text() => Encoding.Unicode.GetString(Block());

    // A length in bytes, then bytes we do not need
    public void SkipAnsiText() => Block();

    // The strings of an entry, then its fixed block. Returns the readable strings and the block.
    public (string[] Strings, byte[] Tail) Entry(InnoShape shape)
    {
        var strings = new string[shape.Strings];
        for (int i = 0; i < strings.Length; i++) strings[i] = Text();

        for (int i = 0; i < shape.AnsiStrings; i++) SkipAnsiText();

        return (strings, Bytes(shape.Tail));
    }

    byte[] Block()
    {
        int length = Int32();
        if (length < 0 || length > MaxStringBytes) throw new InvalidDataException("the Inno Setup data is damaged");
        return Bytes(length);
    }
}
