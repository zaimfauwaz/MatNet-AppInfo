using System.IO;

namespace AppInfoGUI.Package.Cab;

// All the bytes a decoder has produced so far. Matches in packed data point back into it.
internal class GrowingBuffer
{
    byte[] _data = new byte[64 * 1024];

    public int Length { get; private set; }

    public byte this[int index] => _data[index];

    public void Append(byte value)
    {
        Ensure(Length + 1);
        _data[Length++] = value;
    }

    public void Append(byte[] source, int offset, int count)
    {
        Ensure(Length + count);
        Array.Copy(source, offset, _data, Length, count);
        Length += count;
    }

    // Copies `count` bytes that start `distance` bytes back. The copy may overlap itself.
    public void CopyMatch(int distance, int count)
    {
        if (distance <= 0 || distance > Length) throw new InvalidDataException("the packed data is damaged");

        Ensure(Length + count);
        for (int i = 0; i < count; i++) _data[Length + i] = _data[Length + i - distance];
        Length += count;
    }

    public byte[] ToArray(int start, int count)
    {
        var bytes = new byte[count];
        Array.Copy(_data, start, bytes, 0, count);
        return bytes;
    }

    void Ensure(int size)
    {
        if (size <= _data.Length) return;
        Array.Resize(ref _data, Math.Max(size, _data.Length * 2));
    }
}
