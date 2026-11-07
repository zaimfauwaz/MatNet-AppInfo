using System.IO;

namespace AppInfoGUI.Driver;

// A list of known vulnerable drivers, kept as a plain file next to the app's data.
// Only hashes are stored, so a driver is recognised by what it is, not by its name.
internal class VulnerableDriverList
{
    public const string FileName = "vulnerable-drivers.json";

    readonly Dictionary<string, string> _fileHashes = new(StringComparer.OrdinalIgnoreCase);
    readonly Dictionary<string, string> _signedHashes = new(StringComparer.OrdinalIgnoreCase);

    public DateTime Dated { get; private set; }
    public int Count => _fileHashes.Count + _signedHashes.Count;

    // The name of the driver, or null when the hash is not in the list
    public string? FindByFileHash(string sha256) => _fileHashes.GetValueOrDefault(sha256);
    public string? FindBySignedHash(string sha256) => _signedHashes.GetValueOrDefault(sha256);

    public void AddFileHash(string sha256, string name) => _fileHashes[sha256] = name;
    public void AddSignedHash(string sha256, string name) => _signedHashes[sha256] = name;

    // The user's copy wins, so the list can be updated without touching the app's folder
    public static string UserPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MatNet AppInfo", FileName);

    static string BundledPath => Path.Combine(AppContext.BaseDirectory, "Data", FileName);

    // Returns null when there is no list on this PC
    public static VulnerableDriverList? Open()
    {
        string? path = new[] { UserPath, BundledPath }.FirstOrDefault(File.Exists);
        return path == null ? null : Load(path);
    }

    public static VulnerableDriverList Load(string path)
    {
        var list = new VulnerableDriverList { Dated = File.GetLastWriteTime(path) };
        VulnerableDriverParser.Fill(list, File.ReadAllText(path));
        return list;
    }
}
