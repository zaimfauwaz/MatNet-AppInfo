using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace AppInfoGUI.Info;

public static class DefaultIcon
{
    // The icon Windows shows for this kind of file. Returns null if Windows gives none.
    public static IconImage? Read(string path)
    {
        // Only the extension is used, so the file itself is not touched
        var info = new ShellIconNative.FileInfo();
        IntPtr result = ShellIconNative.SHGetFileInfo(Path.GetFileName(path), ShellIconNative.NormalFileAttribute, ref info,
            (uint)Marshal.SizeOf(info), ShellIconNative.UseFileAttributes | ShellIconNative.SysIconIndex);
        if (result == IntPtr.Zero) return null;

        Guid imageListId = typeof(ShellIconNative.IImageList).GUID;
        if (ShellIconNative.SHGetImageList(ShellIconNative.Jumbo, ref imageListId, out var list) != 0)
            return null;

        if (list.GetIcon(info.IconIndex, 0, out IntPtr handle) != 0 || handle == IntPtr.Zero)
            return null;

        try
        {
            BitmapSource image = Imaging.CreateBitmapSourceFromHIcon(handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            image.Freeze();   // needed to hand the image to the window from a background thread

            string extension = Path.GetExtension(path).ToLowerInvariant();
            return new IconImage(image,
                $"Windows default icon for {extension} files, {image.PixelWidth} x {image.PixelHeight} px (this file has no icon of its own)");
        }
        finally
        {
            ShellIconNative.DestroyIcon(handle);
        }
    }
}
