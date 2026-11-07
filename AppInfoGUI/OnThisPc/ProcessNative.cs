using System.Runtime.InteropServices;
using System.Text;

namespace AppInfoGUI.OnThisPc;

internal static class ProcessNative
{
    const uint QueryLimitedInformation = 0x1000;

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr OpenProcess(uint access, bool inherit, int processId);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool CloseHandle(IntPtr handle);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern bool QueryFullProcessImageName(IntPtr process, uint flags, StringBuilder name, ref uint size);

    // The file a process was started from, or null when Windows does not allow us to ask
    public static string? ImagePath(int processId)
    {
        IntPtr handle = OpenProcess(QueryLimitedInformation, false, processId);
        if (handle == IntPtr.Zero) return null;

        try
        {
            var name = new StringBuilder(1024);
            uint size = 1024;
            return QueryFullProcessImageName(handle, 0, name, ref size) ? name.ToString() : null;
        }
        finally
        {
            CloseHandle(handle);
        }
    }
}
