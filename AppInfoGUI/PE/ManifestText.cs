using AppInfoGUI.Info;
using System.Runtime.InteropServices;
using System.Text;

namespace AppInfoGUI.PE;

// Gets the manifest text embedded in a file. The file is loaded as data only, never run.
internal static class ManifestText
{
    static readonly IntPtr ManifestType = (IntPtr)24;   // RT_MANIFEST

    // Returns null when the file has no embedded manifest
    public static string? Read(string path)
    {
        IntPtr module = IconNative.LoadLibraryEx(path, IntPtr.Zero, IconNative.LoadAsDataFile);
        if (module == IntPtr.Zero) throw new InvalidOperationException("Windows could not open the file's resources");

        try
        {
            IntPtr? name = FirstName(module);
            return name == null ? null : ReadResource(module, name.Value);
        }
        finally
        {
            IconNative.FreeLibrary(module);
        }
    }

    static IntPtr? FirstName(IntPtr module)
    {
        IntPtr? found = null;

        IconNative.EnumResourceNames(module, ManifestType, (m, type, name, param) =>
        {
            found = name;
            return false;   // the first one is enough
        }, IntPtr.Zero);

        return found;
    }

    static string? ReadResource(IntPtr module, IntPtr name)
    {
        IntPtr resource = IconNative.FindResource(module, name, ManifestType);
        if (resource == IntPtr.Zero) return null;

        uint size = IconNative.SizeofResource(module, resource);
        IntPtr data = IconNative.LoadResource(module, resource);
        if (size == 0 || data == IntPtr.Zero) return null;

        var bytes = new byte[size];
        Marshal.Copy(IconNative.LockResource(data), bytes, 0, (int)size);

        // Manifests are normally UTF-8, sometimes with a byte order mark
        return Encoding.UTF8.GetString(bytes).TrimStart('﻿');
    }
}
