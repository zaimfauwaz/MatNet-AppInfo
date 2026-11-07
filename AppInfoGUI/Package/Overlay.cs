using System.Reflection.PortableExecutable;
using System.IO;

namespace AppInfoGUI.Package;

// The data stuck on after the end of a program's own sections (but before its digital signature).
// Installers and self-extracting archives keep their packed files here.
public record Overlay(long Start, long End)
{
    public long Length => End - Start;

    // Null when the file has nothing after its sections
    public static Overlay? Find(PEReader pe, Stream stream)
    {
        long fileLength = stream.Length;
        PEHeader? optional = pe.PEHeaders.PEHeader;
        if (optional == null) return null;

        long end = optional.SizeOfHeaders;
        foreach (SectionHeader section in pe.PEHeaders.SectionHeaders)
            end = Math.Max(end, (long)section.PointerToRawData + section.SizeOfRawData);

        end = Math.Max(end, DebugSymbolsEnd(pe.PEHeaders.CoffHeader, stream));

        // The certificate table is the one directory entry that holds a file position, not an address
        long certificate = optional.CertificateTableDirectory.RelativeVirtualAddress;
        long limit = certificate > 0 && certificate < fileLength ? certificate : fileLength;

        return limit > end ? new Overlay(end, limit) : null;
    }

    // Some compilers leave a symbol table behind the sections. That is debug data, not a package.
    static long DebugSymbolsEnd(CoffHeader coff, Stream stream)
    {
        if (coff.PointerToSymbolTable <= 0) return 0;

        long symbolsEnd = coff.PointerToSymbolTable + coff.NumberOfSymbols * 18L;
        if (symbolsEnd + 4 > stream.Length) return 0;

        // A table of long names follows the symbols. Its first four bytes are its own size.
        var size = new byte[4];
        stream.Position = symbolsEnd;
        stream.ReadExactly(size, 0, 4);
        return symbolsEnd + BitConverter.ToUInt32(size, 0);
    }
}
