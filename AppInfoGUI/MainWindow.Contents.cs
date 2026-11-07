using AppInfoGUI.Installer;
using System.IO;

namespace AppInfoGUI
{
    // The "What it installs" tab (.msi only)
    public partial class MainWindow
    {
        async Task LoadContentsAsync(string path)
        {
            if (!path.EndsWith(".msi", StringComparison.OrdinalIgnoreCase) || !File.Exists(path))
                return;

            ContentsMessage.Text = "Reading the package...";

            try
            {
                InstallContents contents = await Task.Run(() => ContentsReader.Read(path));
                ShowContents(contents);
            }
            catch (Exception ex)
            {
                ContentsMessage.Text = "Could not read the package: " + ex.Message;
            }
        }

        void ShowContents(InstallContents contents)
        {
            FilesGrid.ItemsSource = contents.Files.Rows;
            RegistryGrid.ItemsSource = contents.Registry.Rows;
            ServicesGrid.ItemsSource = contents.Services.Rows;
            ActionsGrid.ItemsSource = contents.Actions.Rows;

            FilesSubTab.Header = $"Files ({contents.Files.Total})";
            RegistrySubTab.Header = $"Registry ({contents.Registry.Total})";
            ServicesSubTab.Header = $"Services ({contents.Services.Total})";
            ActionsSubTab.Header = $"Custom actions ({contents.Actions.Total})";

            ContentsMessage.Text = contents.Message != "" ? contents.Message
                : "This is what the package is set up to add. Nothing has been installed.";
        }
    }
}
