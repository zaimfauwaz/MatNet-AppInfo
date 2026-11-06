using System.Runtime.InteropServices;

namespace AppInfoGUI.Info;

// Windows' own default icon for a file type (what Explorer shows)
internal static class ShellIconNative
{
    public const uint UseFileAttributes = 0x10;   // the file does not need to be read, only its name
    public const uint SysIconIndex = 0x4000;
    public const int NormalFileAttribute = 0x80;
    public const int Jumbo = 4;                   // 256 x 256, the biggest Windows offers

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct FileInfo
    {
        public IntPtr Icon;
        public int IconIndex;
        public uint Attributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] public string DisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)] public string TypeName;
    }

    [ComImport, Guid("46EB5926-582E-4017-9FDF-E8998DAA0950"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IImageList
    {
        // Only GetIcon is used. The others are listed so GetIcon sits in the right place.
        [PreserveSig] int Add(IntPtr image, IntPtr mask, ref int index);
        [PreserveSig] int ReplaceIcon(int index, IntPtr icon, ref int result);
        [PreserveSig] int SetOverlayImage(int image, int overlay);
        [PreserveSig] int Replace(int index, IntPtr image, IntPtr mask);
        [PreserveSig] int AddMasked(IntPtr image, int maskColor, ref int index);
        [PreserveSig] int Draw(IntPtr parameters);
        [PreserveSig] int Remove(int index);
        [PreserveSig] int GetIcon(int index, int flags, out IntPtr icon);
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr SHGetFileInfo(string path, int attributes, ref FileInfo info, uint size, uint flags);

    [DllImport("shell32.dll")]
    public static extern int SHGetImageList(int imageList, ref Guid riid, out IImageList list);

    [DllImport("user32.dll")]
    public static extern bool DestroyIcon(IntPtr icon);
}
