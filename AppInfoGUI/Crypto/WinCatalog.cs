using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace AppInfoGUI.Crypto;

internal static class WinCatalog
{
    public const uint ChoiceCatalog = 2;

    [DllImport("wintrust.dll", ExactSpelling = true)]
    public static extern bool CryptCATAdminAcquireContext2(
        out IntPtr catAdmin, IntPtr subsystem,
        [MarshalAs(UnmanagedType.LPWStr)] string hashAlgorithm,
        IntPtr strongHashPolicy, uint flags);

    [DllImport("wintrust.dll", ExactSpelling = true)]
    public static extern bool CryptCATAdminReleaseContext(IntPtr catAdmin, uint flags);

    [DllImport("wintrust.dll", ExactSpelling = true)]
    public static extern bool CryptCATAdminCalcHashFromFileHandle2(
        IntPtr catAdmin, SafeFileHandle fileHandle, ref uint hashSize, byte[]? hash, uint flags);

    [DllImport("wintrust.dll", ExactSpelling = true)]
    public static extern IntPtr CryptCATAdminEnumCatalogFromHash(
        IntPtr catAdmin, byte[] hash, uint hashSize, uint flags, IntPtr previousCatInfo);

    [DllImport("wintrust.dll", ExactSpelling = true)]
    public static extern bool CryptCATAdminReleaseCatalogContext(
        IntPtr catAdmin, IntPtr catInfo, uint flags);

    [DllImport("wintrust.dll", ExactSpelling = true)]
    public static extern bool CryptCATCatalogInfoFromContext(
        IntPtr catInfo, ref CatalogInfo info, uint flags);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct CatalogInfo
    {
        public uint Size;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] public string CatalogFile;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WinTrustCatalogInfo
    {
        public uint Size;
        public uint CatalogVersion;
        [MarshalAs(UnmanagedType.LPWStr)] public string CatalogFilePath;
        [MarshalAs(UnmanagedType.LPWStr)] public string MemberTag;
        [MarshalAs(UnmanagedType.LPWStr)] public string MemberFilePath;
        public IntPtr MemberFile;
        public IntPtr CalculatedFileHash;
        public uint CalculatedFileHashSize;
        public IntPtr CatalogContext;
        public IntPtr CatAdmin;
    }
}