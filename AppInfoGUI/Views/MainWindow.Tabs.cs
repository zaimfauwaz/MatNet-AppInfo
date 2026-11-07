using AppInfoGUI.Info;

namespace AppInfoGUI
{
    // Which tabs and sections are shown depends on the file type
    public partial class MainWindow
    {
        string _filePath = "";                // the file this window was opened for
        FileKind _kind = new FileKind("");    // what kind of file it is

        void ApplyFileType()
        {
            DetailsTab.Header = _kind.DetailsTabTitle;

            // Installer packages are not PE files, so these tabs would be empty
            PeTab.Visibility = VisibleIf(!_kind.IsInstaller);
            DependenciesTab.Visibility = VisibleIf(!_kind.IsInstaller);
            OnPcTab.Visibility = VisibleIf(!_kind.IsInstaller);
            InstallsTab.Visibility = VisibleIf(_kind.IsMsi);

            // Installers have their own sections instead of the file version ones
            VersionSection.Visibility = VisibleIf(!_kind.IsInstaller);
            InstallerSection.Visibility = VisibleIf(_kind.IsInstaller);
            LibrarySection.Visibility = VisibleIf(_kind.IsLibrary);
            DriverSection.Visibility = VisibleIf(_kind.IsDriver);
        }

        static System.Windows.Visibility VisibleIf(bool visible) =>
            visible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
    }
}
