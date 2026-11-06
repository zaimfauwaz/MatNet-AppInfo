using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;

namespace AppInfoGUI.Info;

public record IconImage(ImageSource Image, string Description);

public static class IconReader
{
    // Returns null when the file has no icon or the icon cannot be read
    public static IconImage? Read(string path)
    {
        IntPtr module = IconNative.LoadLibraryEx(path, IntPtr.Zero,
            IconNative.LoadAsDataFile | IconNative.LoadAsImageResource);
        if (module == IntPtr.Zero) return null;

        try
        {
            IntPtr groupName = FirstGroupName(module);
            if (groupName == IntPtr.Zero) return null;

            byte[]? ico = BuildIcoFile(module, groupName);
            return ico == null ? null : Largest(ico);
        }
        finally
        {
            IconNative.FreeLibrary(module);
        }
    }

    // The first icon group is the one Windows shows for the file
    static IntPtr FirstGroupName(IntPtr module)
    {
        IntPtr first = IntPtr.Zero;

        IconNative.EnumResourceNames(module, IconNative.IconGroup, (m, type, name, param) =>
        {
            first = name;
            return false;   // stop after the first one
        }, IntPtr.Zero);

        return first;
    }

    // An .exe stores icon sizes as separate pieces. This joins them into a normal .ico file in memory.
    static byte[]? BuildIcoFile(IntPtr module, IntPtr groupName)
    {
        byte[]? group = GetResource(module, groupName, IconNative.IconGroup);
        if (group == null || group.Length < 6) return null;

        int count = BitConverter.ToUInt16(group, 4);
        var pieces = new List<(byte[] Header, byte[] Data)>();

        for (int i = 0; i < count; i++)
        {
            int at = 6 + i * 14;
            if (at + 14 > group.Length) break;

            ushort id = BitConverter.ToUInt16(group, at + 12);
            byte[]? data = GetResource(module, (IntPtr)id, IconNative.Icon);
            if (data == null) continue;   // one broken size must not hide the others

            byte[] header = new byte[12];
            Array.Copy(group, at, header, 0, 12);   // width, height, colors, planes, bits, size
            pieces.Add((header, data));
        }

        if (pieces.Count == 0) return null;

        using var ico = new MemoryStream();
        using var writer = new BinaryWriter(ico);

        writer.Write((ushort)0);
        writer.Write((ushort)1);
        writer.Write((ushort)pieces.Count);

        int offset = 6 + pieces.Count * 16;
        foreach (var piece in pieces)
        {
            writer.Write(piece.Header);
            writer.Write(offset);
            offset += piece.Data.Length;
        }
        foreach (var piece in pieces)
            writer.Write(piece.Data);

        return ico.ToArray();
    }

    static byte[]? GetResource(IntPtr module, IntPtr name, IntPtr type)
    {
        IntPtr found = IconNative.FindResource(module, name, type);
        if (found == IntPtr.Zero) return null;

        IntPtr loaded = IconNative.LoadResource(module, found);
        IntPtr locked = IconNative.LockResource(loaded);
        uint size = IconNative.SizeofResource(module, found);
        if (locked == IntPtr.Zero || size == 0) return null;

        byte[] bytes = new byte[size];
        Marshal.Copy(locked, bytes, 0, (int)size);
        return bytes;
    }

    // Picks the biggest picture. On a tie, the one with more colors.
    static IconImage? Largest(byte[] ico)
    {
        var decoder = new IconBitmapDecoder(new MemoryStream(ico),
            BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);

        BitmapFrame? best = null;
        foreach (BitmapFrame frame in decoder.Frames)
        {
            if (best == null
                || frame.PixelWidth * frame.PixelHeight > best.PixelWidth * best.PixelHeight
                || (frame.PixelWidth == best.PixelWidth && frame.Format.BitsPerPixel > best.Format.BitsPerPixel))
                best = frame;
        }

        if (best == null) return null;

        best.Freeze();   // needed to hand the image to the window from a background thread
        return new IconImage(best,
            $"Largest icon in file: {best.PixelWidth} x {best.PixelHeight} px (of {decoder.Frames.Count} sizes)");
    }
}
