namespace AppInfoGUI.Info;

internal static class HiddenStreams
{
    static readonly IntPtr InvalidHandle = new IntPtr(-1);

    // Names of the extra streams on a file. The normal download mark is not counted.
    public static string Describe(string path)
    {
        IntPtr handle = StreamNative.FindFirstStreamW(path, 0, out var data, 0);
        if (handle == InvalidHandle)
            return "Not available (this drive may not support streams)";

        var names = new List<string>();
        try
        {
            do
            {
                string name = StreamName(data.Name);
                if (name != "" && name != "Zone.Identifier")
                    names.Add(name);
            }
            while (StreamNative.FindNextStreamW(handle, out data));
        }
        finally
        {
            StreamNative.FindClose(handle);
        }

        return names.Count == 0 ? "None" : "WARNING: " + string.Join(", ", names);
    }

    // Windows writes ":name:$DATA". The file's own content is "::$DATA", which gives an empty name.
    static string StreamName(string raw)
    {
        string name = raw.Length > 0 ? raw.Substring(1) : raw;
        return name.EndsWith(":$DATA") ? name.Substring(0, name.Length - 6) : name;
    }
}
