using System.IO;
using AppInfoGUI.Info;

namespace AppInfoGUI.Installer;

// Reads an .msi or .msp. Nothing is installed or run.
public static class InstallerReader
{
    public static InstallerInfo Read(string path)
    {
        var info = new InstallerInfo { Title = Path.GetFileName(path) };
        bool isPatch = Path.GetExtension(path).Equals(".msp", StringComparison.OrdinalIgnoreCase);

        MsiSummary? summary = Try(() => new MsiSummary(path));
        MsiPackage? package = isPatch ? null : Try(() => new MsiPackage(path));

        try
        {
            if (summary == null && package == null)
            {
                info.Product = "Could not read this installer package";
                return info;
            }

            Dictionary<string, string> props = new();
            if (package != null)
            {
                props = Try(() => package.Properties()) ?? props;
                MsiProductRows.Fill(info.ProductRows, props);
            }

            if (summary != null) MsiPackageRows.FillSummary(info.PackageRows, summary, isPatch);
            if (package != null) MsiPackageRows.FillContents(info.PackageRows, package);

            FillHeader(info, props, summary, isPatch);
        }
        finally
        {
            summary?.Dispose();
            package?.Dispose();
        }

        return info;
    }

    static void FillHeader(InstallerInfo info, Dictionary<string, string> props, MsiSummary? summary, bool isPatch)
    {
        string name = props.GetValueOrDefault("ProductName") ?? "";
        string version = props.GetValueOrDefault("ProductVersion") ?? "";
        string maker = props.GetValueOrDefault("Manufacturer") ?? summary?.Text(MsiSummary.Author) ?? "";

        string title = name != "" ? name : summary?.Text(MsiSummary.Title) ?? "";
        if (title.Trim() != "") info.Title = title;

        info.Product = isPatch ? "Windows Installer patch" : $"{name} {version}".Trim();
        info.Company = maker.Trim() != "" ? maker : "Company not stated";
    }

    // A failed read gives null, so one broken source never blocks the others
    static T? Try<T>(Func<T> read) where T : class
    {
        try
        {
            return read();
        }
        catch (Exception)
        {
            return null;
        }
    }
}
