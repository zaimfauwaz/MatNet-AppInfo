using System.Text;

namespace AppInfoGUI.Installer;

// The "summary" page of a package: title, author, platform, dates
internal sealed class MsiSummary : IDisposable
{
    public const uint Title = 2, Author = 4, Comments = 6, Template = 7, Revision = 9,
                      Created = 12, LastSaved = 13, InstallerVersion = 14, CreatedWith = 18;

    const uint TypeInt16 = 2, TypeInt32 = 3, TypeText = 30, TypeTime = 64;

    readonly IntPtr _handle;

    public MsiSummary(string path)
    {
        if (MsiNative.MsiGetSummaryInformation(IntPtr.Zero, path, 0, out _handle) != 0)
            throw new InvalidOperationException("Could not read the package summary");
    }

    public string? Text(uint property)
    {
        var buffer = new StringBuilder(4096);
        uint size = 4096;

        uint result = MsiNative.MsiSummaryInfoGetProperty(_handle, property, out uint type, out _, out _, buffer, ref size);
        return result == 0 && type == TypeText ? buffer.ToString() : null;
    }

    public int? Number(uint property)
    {
        var buffer = new StringBuilder(1);
        uint size = 1;

        uint result = MsiNative.MsiSummaryInfoGetProperty(_handle, property, out uint type, out int value, out _, buffer, ref size);
        return result == 0 && (type == TypeInt16 || type == TypeInt32) ? value : null;
    }

    public DateTime? Time(uint property)
    {
        var buffer = new StringBuilder(1);
        uint size = 1;

        uint result = MsiNative.MsiSummaryInfoGetProperty(_handle, property, out uint type, out _, out long fileTime, buffer, ref size);
        return result == 0 && type == TypeTime && fileTime > 0 ? DateTime.FromFileTimeUtc(fileTime).ToLocalTime() : null;
    }

    public void Dispose() => MsiNative.MsiCloseHandle(_handle);
}
