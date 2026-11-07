using System.IO;

namespace AppInfoGUI.Package.Streams;

// Gives at most `limit` bytes of another stream, and counts how many were taken.
internal class LimitedStream : Stream
{
    readonly Stream _inner;
    readonly long _limit;

    public long Taken { get; private set; }

    public LimitedStream(Stream inner, long limit)
    {
        _inner = inner;
        _limit = limit;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        count = (int)Math.Min(count, _limit - Taken);
        if (count <= 0) return 0;

        int read = _inner.Read(buffer, offset, count);
        Taken += read;
        return read;
    }

    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => throw new NotSupportedException();
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override void Flush() { }
}
