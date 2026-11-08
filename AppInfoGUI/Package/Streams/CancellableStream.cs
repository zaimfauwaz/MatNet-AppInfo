using System.IO;

namespace AppInfoGUI.Package.Streams;

// Passes reads on to another stream, but stops with an OperationCanceledException once cancelled.
// Every reader works through the one file stream, so wrapping it makes all of them cancellable.
public class CancellableStream : Stream
{
    readonly Stream _inner;
    readonly CancellationToken _token;

    public CancellableStream(Stream inner, CancellationToken token)
    {
        _inner = inner;
        _token = token;
    }

    public override bool CanRead => _inner.CanRead;
    public override bool CanSeek => _inner.CanSeek;
    public override bool CanWrite => false;
    public override long Length => _inner.Length;

    public override long Position
    {
        get => _inner.Position;
        set => _inner.Position = value;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        _token.ThrowIfCancellationRequested();
        return _inner.Read(buffer, offset, count);
    }

    public override int Read(Span<byte> buffer)
    {
        _token.ThrowIfCancellationRequested();
        return _inner.Read(buffer);
    }

    public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);

    public override void Flush() { }
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
}
