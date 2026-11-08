using System.IO;

namespace AppInfoGUI.Package.Streams;

// A read-only window onto part of a file, so an archive that starts in the middle
// of a program can be opened as if it were a file of its own.
internal class SubStream : Stream
{
    readonly Stream _inner;
    readonly long _start;
    readonly long _length;

    public SubStream(Stream inner, long start, long length)
    {
        _inner = inner;
        _start = start;
        _length = length;
    }

    public override bool CanRead => true;
    public override bool CanSeek => true;
    public override bool CanWrite => false;
    public override long Length => _length;

    public override long Position { get; set; }

    public override int Read(byte[] buffer, int offset, int count)
    {
        count = (int)Math.Min(count, Math.Max(0, _length - Position));
        if (count == 0) return 0;

        _inner.Position = _start + Position;
        int read = _inner.Read(buffer, offset, count);
        Position += read;
        return read;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        Position = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => Position + offset,
            _ => _length + offset
        };
        return Position;
    }

    public override void Flush() { }
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
}
