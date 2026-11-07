using AppInfoGUI.Package.Streams;
using System.IO;

namespace AppInfoGUI.Package.Nsis;

// Gets the installer's script header out of the packed data.
// In a "solid" installer the whole data is one packed stream. Otherwise the header is packed on its own.
public static class NsisHeaderReader
{
    public static NsisHeaderData Read(Stream file, NsisFirstHeader first)
    {
        long length = Math.Min(first.DataLength, file.Length - first.DataStart);
        if (length < 8) throw new InvalidDataException("the NSIS data is too short");

        var data = new SubStream(file, first.DataStart, length);
        var lead = new byte[8];
        data.ReadExactly(lead, 0, 8);

        // LZMA and bzip2 streams start with a marker, so this is a solid installer
        if (NsisCompression.Announced(lead) != null) return Solid(data, first.HeaderLength);

        // Otherwise the data starts with the size of the header block. The top bit means "packed".
        uint word = BitConverter.ToUInt32(lead, 0);
        bool packed = (word & 0x80000000) != 0;
        long size = word & 0x7FFFFFFF;

        // The files follow the header block
        long filesStart = first.DataStart + 4 + size;

        if (packed && size <= length - 4)
            return new NsisHeaderData(Unpack(new SubStream(data, 4, size), first.HeaderLength), filesStart);

        if (!packed && size == first.HeaderLength)
            return new NsisHeaderData(ReadBytes(new SubStream(data, 4, size), first.HeaderLength), filesStart);

        return Solid(data, first.HeaderLength);   // solid with deflate, which has no marker
    }

    // The header is the first thing in the packed stream, after its own length
    static NsisHeaderData Solid(Stream data, int headerLength)
    {
        data.Position = 0;
        using Stream unpacked = NsisCompression.Open(data);

        var size = new byte[4];
        unpacked.ReadExactly(size, 0, 4);
        if (BitConverter.ToInt32(size) != headerLength)
            throw new InvalidDataException("the NSIS header does not have the expected length");

        return new NsisHeaderData(ReadBytes(unpacked, headerLength), null);
    }

    static byte[] Unpack(Stream packed, int length)
    {
        using Stream unpacked = NsisCompression.Open(packed);
        return ReadBytes(unpacked, length);
    }

    static byte[] ReadBytes(Stream stream, int length)
    {
        var bytes = new byte[length];
        stream.ReadExactly(bytes, 0, length);
        return bytes;
    }
}
