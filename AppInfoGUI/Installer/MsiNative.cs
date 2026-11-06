using System.Runtime.InteropServices;
using System.Text;

namespace AppInfoGUI.Installer;

// Windows' own installer library. These calls only read the package, they never install it.
internal static class MsiNative
{
    [DllImport("msi.dll", CharSet = CharSet.Unicode)]
    public static extern uint MsiOpenDatabase(string path, IntPtr persist, out IntPtr database);

    [DllImport("msi.dll", CharSet = CharSet.Unicode)]
    public static extern uint MsiDatabaseOpenView(IntPtr database, string query, out IntPtr view);

    [DllImport("msi.dll")]
    public static extern uint MsiViewExecute(IntPtr view, IntPtr record);

    [DllImport("msi.dll")]
    public static extern uint MsiViewFetch(IntPtr view, out IntPtr record);

    [DllImport("msi.dll", CharSet = CharSet.Unicode)]
    public static extern uint MsiRecordGetString(IntPtr record, uint field, StringBuilder buffer, ref uint size);

    [DllImport("msi.dll", CharSet = CharSet.Unicode)]
    public static extern uint MsiGetSummaryInformation(IntPtr database, string path, uint updateCount, out IntPtr summary);

    [DllImport("msi.dll", CharSet = CharSet.Unicode)]
    public static extern uint MsiSummaryInfoGetProperty(IntPtr summary, uint property,
        out uint dataType, out int intValue, out long fileTime, StringBuilder buffer, ref uint size);

    [DllImport("msi.dll")]
    public static extern uint MsiCloseHandle(IntPtr handle);
}
