namespace AppInfoGUI.Package;

// The first bytes of each archive type
public static class Signatures
{
    public static readonly byte[] SevenZip = { 0x37, 0x7A, 0xBC, 0xAF, 0x27, 0x1C };
    public static readonly byte[] Rar = { 0x52, 0x61, 0x72, 0x21, 0x1A, 0x07 };   // "Rar!" 1A 07
    public static readonly byte[] Zip = { 0x50, 0x4B, 0x03, 0x04 };               // "PK" 03 04
}
