using System.IO;
using System.Text;

namespace AppInfoGUI.Package.Archives;

// Reads the list of files from a ZIP's directory at the end of the archive. Nothing is unpacked.
// It also works when the ZIP sits behind a program, as in a self-extracting archive.
internal class ZipLister : IPackageLister
{
    static readonly byte[] EndSignature = { 0x50, 0x4B, 0x05, 0x06 };
    const int EndRecordSize = 22;
    const int MaxDirectoryBytes = 64 * 1024 * 1024;

    public List<PackageEntry> List(Stream file, Overlay overlay)
    {
        // The end record is in the last 64 KB (it can be followed by a comment)
        long tailLength = Math.Min(overlay.Length, 65535 + EndRecordSize);
        byte[] tail = ReadAt(file, overlay.End - tailLength, (int)tailLength);

        int end = tail.AsSpan().LastIndexOf(EndSignature);
        if (end < 0 || end + EndRecordSize > tail.Length)
            throw new InvalidDataException("the ZIP has no end-of-archive record");

        int count = BitConverter.ToUInt16(tail, end + 10);
        uint size = BitConverter.ToUInt32(tail, end + 12);
        if (count == 0xFFFF || size == 0xFFFFFFFF)
            throw new NotSupportedException("very large ZIP64 archives are not supported yet");
        if (size > MaxDirectoryBytes)
            throw new InvalidDataException("the ZIP directory is unusually large");

        // The directory sits right before the end record. Counting back from there
        // works even when the file has a program in front of the ZIP.
        long directoryStart = overlay.End - tailLength + end - size;
        if (directoryStart < overlay.Start)
            throw new InvalidDataException("the ZIP directory is damaged");

        return Parse(ReadAt(file, directoryStart, (int)size), count);
    }

    static List<PackageEntry> Parse(byte[] data, int count)
    {
        var entries = new List<PackageEntry>();
        int at = 0;

        for (int i = 0; i < count && at + 46 <= data.Length; i++)
        {
            if (BitConverter.ToUInt32(data, at) != 0x02014B50)
                throw new InvalidDataException("the ZIP directory is damaged");

            ushort flags = BitConverter.ToUInt16(data, at + 8);
            uint size = BitConverter.ToUInt32(data, at + 24);
            int nameLength = BitConverter.ToUInt16(data, at + 28);
            int extraLength = BitConverter.ToUInt16(data, at + 30);
            int commentLength = BitConverter.ToUInt16(data, at + 32);

            // Bit 11 means the name is UTF-8. Otherwise it is an old DOS name, plain letters in practice.
            Encoding encoding = (flags & 0x0800) != 0 ? Encoding.UTF8 : Encoding.Latin1;
            string name = encoding.GetString(data, at + 46, nameLength);

            entries.Add(new PackageEntry(name, size, EntryClassifier.Classify(name), Encrypted: (flags & 1) != 0));
            at += 46 + nameLength + extraLength + commentLength;
        }

        return entries;
    }

    static byte[] ReadAt(Stream stream, long position, int length)
    {
        var buffer = new byte[length];
        stream.Position = position;
        stream.ReadExactly(buffer, 0, length);
        return buffer;
    }
}
