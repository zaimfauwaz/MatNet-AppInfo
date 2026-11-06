using AppInfoGUI.Info;

namespace AppInfoGUI.Installer;

// Facts about the package file itself, from its summary page and tables
internal static class MsiPackageRows
{
    public static void FillSummary(List<DetailRow> rows, MsiSummary summary, bool isPatch)
    {
        rows.Add(new("Title", Safe.Text(() => summary.Text(MsiSummary.Title))));
        rows.Add(new("Author", Safe.Text(() => summary.Text(MsiSummary.Author))));
        rows.Add(new("Comments", Safe.Text(() => summary.Text(MsiSummary.Comments))));
        rows.Add(new("Created with", Safe.Text(() => summary.Text(MsiSummary.CreatedWith))));
        rows.Add(new("Created", Safe.Text(() => Date(summary.Time(MsiSummary.Created)))));
        rows.Add(new("Last saved", Safe.Text(() => Date(summary.Time(MsiSummary.LastSaved)))));
        rows.Add(new("Minimum installer version", Safe.Text(() => InstallerVersion(summary.Number(MsiSummary.InstallerVersion)))));

        // The same two fields mean different things in a patch
        if (isPatch)
        {
            rows.Add(new("Patch code", Safe.Text(() => summary.Text(MsiSummary.Revision))));
            rows.Add(new("Applies to products", Safe.Text(() => summary.Text(MsiSummary.Template))));
        }
        else
        {
            rows.Add(new("Package code", Safe.Text(() => summary.Text(MsiSummary.Revision))));
            rows.Add(new("Platform", Safe.Text(() => Platform(summary.Text(MsiSummary.Template)))));
        }
    }

    public static void FillContents(List<DetailRow> rows, MsiPackage package)
    {
        rows.Add(new("Files in package", Safe.Text(() => package.CountRows("File", "File").ToString())));
        rows.Add(new("Custom actions", Safe.Text(() => CustomActions(package.CountRows("CustomAction", "Action")))));
    }

    static string? Date(DateTime? time) => time?.ToString("yyyy-MM-dd HH:mm");

    // 500 means version 5.0
    static string? InstallerVersion(int? value) => value == null ? null : $"{value / 100}.{value % 100}";

    // The template reads like "x64;1033": the platform, then the languages
    static string? Platform(string? template)
    {
        string platform = (template ?? "").Split(';')[0].Trim();
        return platform == "" ? "Not stated (usually 32-bit)" : platform;
    }

    static string CustomActions(int count) =>
        count == 0 ? "None" : $"{count} (steps that can run programs during install)";
}
