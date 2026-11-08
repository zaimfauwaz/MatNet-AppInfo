using System.Diagnostics;
using System.IO;

namespace AppInfoGUI.Info;

public static class ApplicationDetails
{
    public static ApplicationInfo Read(string path)
    {
        var info = new ApplicationInfo();
        var file = new FileInfo(path);
        FileVersionInfo? version = ReadVersion(path);

        FillHeader(info, file, version);
        info.Icon = ReadIcon(path);
        FillFile(info, path, file);
        FillVersion(info, file, version);
        FillOrigin(info, path, file);

        return info;
    }

    // A file with no version data gives empty fields. A failed read gives null.
    static FileVersionInfo? ReadVersion(string path)
    {
        try
        {
            return FileVersionInfo.GetVersionInfo(path);
        }
        catch (Exception)
        {
            return null;
        }
    }

    // The file's own icon first, then the Windows default for its type.
    // No icon at all just means the empty box is shown.
    static IconImage? ReadIcon(string path)
    {
        try
        {
            return IconReader.Read(path) ?? DefaultIcon.Read(path);
        }
        catch (Exception)
        {
            return null;
        }
    }

    static void FillHeader(ApplicationInfo info, FileInfo file, FileVersionInfo? version)
    {
        string description = version?.FileDescription ?? "";
        string productName = version?.ProductName ?? "";
        string productVersion = version?.ProductVersion ?? "";
        string company = version?.CompanyName ?? "";

        info.Title = description.Trim() != "" ? description : file.Name;
        info.Product = $"{productName} {productVersion}".Trim();
        info.Company = company.Trim() != "" ? company : "Company not stated";
    }

    static void FillFile(ApplicationInfo info, string path, FileInfo file)
    {
        info.FileRows.Add(new("File name", file.Name));
        info.FileRows.Add(new("Folder", Safe.Text(() => file.DirectoryName)));
        info.FileRows.Add(new("Type by its name", Safe.Text(() => FileTypeDescriber.Describe(path))));
        info.FileRows.Add(new("Type by its contents", Safe.Text(() => FileTypeDescriber.DescribeContents(path))));
        info.FileRows.Add(new("Size", Safe.Text(() => TextFormat.Size(file.Length))));
        info.FileRows.Add(new("Created", Safe.Text(() => file.CreationTime.ToString("yyyy-MM-dd HH:mm"))));
        info.FileRows.Add(new("Modified", Safe.Text(() => file.LastWriteTime.ToString("yyyy-MM-dd HH:mm"))));
        info.FileRows.Add(new("SHA-256", Safe.Text(() => FileHasher.GetSha256(path))));
    }

    static void FillVersion(ApplicationInfo info, FileInfo file, FileVersionInfo? version)
    {
        info.VersionRows.Add(new("File version", Field(version, () => version?.FileVersion)));
        info.VersionRows.Add(new("Product version", Field(version, () => version?.ProductVersion)));
        info.VersionRows.Add(new("Original filename", Field(version, () => TextFormat.OriginalName(version?.OriginalFilename, file.Name))));
        info.VersionRows.Add(new("Internal name", Field(version, () => version?.InternalName)));
        info.VersionRows.Add(new("Copyright", Field(version, () => version?.LegalCopyright)));
        info.VersionRows.Add(new("Trademarks", Field(version, () => version?.LegalTrademarks)));
        info.VersionRows.Add(new("Comments", Field(version, () => version?.Comments)));
        info.VersionRows.Add(new("Language", Field(version, () => version?.Language)));
    }

    // Tells apart "the file says nothing" from "we could not read it"
    static string Field(FileVersionInfo? version, Func<string?> lookup)
    {
        if (version == null) return "Could not read version data";

        try
        {
            string? value = lookup();
            return string.IsNullOrWhiteSpace(value) ? "Not stated by the file" : value;
        }
        catch (Exception)
        {
            return "Could not read";
        }
    }

    static void FillOrigin(ApplicationInfo info, string path, FileInfo file)
    {
        info.OriginRows.Add(new("Mark of the Web", Safe.Text(() => OriginReader.MarkOfTheWeb(path))));
        info.OriginRows.Add(new("Downloaded from", Safe.Text(() => OriginReader.DownloadedFrom(path))));
        info.OriginRows.Add(new("Download link", Safe.Text(() => OriginReader.DownloadLink(path))));
        info.OriginRows.Add(new("Hidden data streams", Safe.Text(() => HiddenStreams.Describe(path))));
        info.OriginRows.Add(new("File attributes", Safe.Text(() => FileFlags.Attributes(path))));
        info.OriginRows.Add(new("Timestamps", Safe.Text(() => FileFlags.Timestamps(file))));
        info.OriginRows.Add(new("Location type", Safe.Text(() => LocationDescriber.Describe(file.DirectoryName ?? ""))));
    }
}
