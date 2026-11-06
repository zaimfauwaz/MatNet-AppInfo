using System.Runtime.InteropServices;

namespace AppInfoGUI.Info;

// Reads resources from a file WITHOUT running it. Windows only loads the data.
internal static class IconNative
{
    public const uint LoadAsDataFile = 0x00000002;
    public const uint LoadAsImageResource = 0x00000020;

    public static readonly IntPtr IconGroup = (IntPtr)14;   // RT_GROUP_ICON
    public static readonly IntPtr Icon = (IntPtr)3;         // RT_ICON

    public delegate bool EnumNameProc(IntPtr module, IntPtr type, IntPtr name, IntPtr param);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern IntPtr LoadLibraryEx(string fileName, IntPtr reserved, uint flags);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool FreeLibrary(IntPtr module);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern bool EnumResourceNames(IntPtr module, IntPtr type, EnumNameProc callback, IntPtr param);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr FindResource(IntPtr module, IntPtr name, IntPtr type);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr LoadResource(IntPtr module, IntPtr resource);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr LockResource(IntPtr resource);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern uint SizeofResource(IntPtr module, IntPtr resource);
}
