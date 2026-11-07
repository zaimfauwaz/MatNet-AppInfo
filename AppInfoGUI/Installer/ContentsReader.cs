namespace AppInfoGUI.Installer;

// Reads what an .msi would put on the PC. Nothing is installed or run.
public static class ContentsReader
{
    const int MaxRows = 5000;

    public static InstallContents Read(string path)
    {
        var contents = new InstallContents();

        try
        {
            using var package = new MsiPackage(path);

            Fill(contents.Files, () => FileEntries.Read(package));
            Fill(contents.Registry, () => RegistryEntries.Read(package));
            Fill(contents.Services, () => ServiceEntries.Read(package));
            Fill(contents.Actions, () => ActionEntries.Read(package));
        }
        catch (Exception ex)
        {
            contents.Message = "Could not read the package: " + ex.Message;
        }

        return contents;
    }

    // Each table is read on its own, so one broken table never hides the others
    static void Fill<T>(InstallSection<T> section, Func<IEnumerable<T>> read)
    {
        try
        {
            List<T> all = read().ToList();
            section.Total = all.Count;
            section.Rows.AddRange(all.Take(MaxRows));
        }
        catch (Exception)
        {
            // The section stays empty
        }
    }
}
