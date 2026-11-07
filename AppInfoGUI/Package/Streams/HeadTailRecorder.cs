using System.IO;

namespace AppInfoGUI.Package.Streams;

// Receives the bytes of a big stream but keeps only its start and its end, so memory stays small
// no matter how large the data is. Nothing is written to disk. Afterwards `ToReadable` gives a
// stream with the real length where only those two parts can be read.
// This is enough for archives whose list of files sits at the end (7z, zip).
internal class HeadTailRecorder : Stream
{
    readonly int _headSize;
    readonly int _tailSize;
    readonly MemoryStream _head = new();
    MemoryStream _tail = new();

    public long Total { get; private set; }

    public HeadTailRecorder(int headSize, int tailSize)
    {
        _headSize = headSize;
        _tailSize = tailSize;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        int forHead = (int)Math.Max(0, Math.Min(count, _headSize - Total));
        _head.Write(buffer, offset, forHead);

        // Everything is also added to the tail, and the tail is cut back from time to time
        _tail.Write(buffer, offset, count);
        Total += count;
        if (_tail.Length > 2L * _tailSize) CutTail();
    }

    void CutTail()
    {
        byte[] bytes = _tail.GetBuffer();
        var kept = new MemoryStream();
        kept.Write(bytes, (int)_tail.Length - _tailSize, _tailSize);
        _tail = kept;
    }

    public Stream ToReadable()
    {
        long tailLength = Math.Min(_tail.Length, _tailSize);
        byte[] bytes = _tail.GetBuffer();
        var tail = new byte[tailLength];
        Array.Copy(bytes, _tail.Length - tailLength, tail, 0, tailLength);
        return new HeadTailView(_head.ToArray(), tail, Total);
    }

    public override bool CanRead => false;
    public override bool CanSeek => false;
    public override bool CanWrite => true;
    public override long Length => Total;
    public override long Position { get => Total; set => throw new NotSupportedException(); }
    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Flush() { }
}
