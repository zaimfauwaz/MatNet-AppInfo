using System.Runtime.InteropServices;

namespace AppInfoGUI.Info;

// Windows calls that list the hidden data streams of an NTFS file
internal static class StreamNative
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct StreamData
    {
        public long Size;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 296)] public string Name;
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern IntPtr FindFirstStreamW(string path, int infoLevel, out StreamData data, int flags);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern bool FindNextStreamW(IntPtr handle, out StreamData data);

    [DllImport("kernel32.dll")]
    public static extern bool FindClose(IntPtr handle);
}
