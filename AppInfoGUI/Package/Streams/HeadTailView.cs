using System.IO;

namespace AppInfoGUI.Package.Streams;

// A read-only stream of a given length of which only the start and the end are known.
// Reading anything in between fails. See HeadTailRecorder.
internal class HeadTailView : Stream
{
    readonly byte[] _head;
    readonly byte[] _tail;
    readonly long _length;
    long _position;

    public HeadTailView(byte[] head, byte[] tail, long length)
    {
        _head = head;
        _tail = tail;
        _length = length;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        count = (int)Math.Min(count, _length - _position);
        if (count <= 0) return 0;

        long tailStart = _length - _tail.Length;
        if (_position < _head.Length)
        {
            count = (int)Math.Min(count, _head.Length - _position);
            Array.Copy(_head, _position, buffer, offset, count);
        }
        else if (_position >= tailStart)
        {
            Array.Copy(_tail, _position - tailStart, buffer, offset, count);
        }
        else
        {
            throw new IOException("this archive is packed inside the installer and its list of files is not at the end");
        }

        _position += count;
        return count;
    }

    public override bool CanRead => true;
    public override bool CanSeek => true;
    public override bool CanWrite => false;
    public override long Length => _length;
    public override long Position { get => _position; set => _position = value; }

    public override long Seek(long offset, SeekOrigin origin)
    {
        _position = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => _position + offset,
            _ => _length + offset
        };
        return _position;
    }

    public override void Flush() { }
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
}
