using System.IO;

namespace AppInfoGUI
{
    // Which tabs are shown depends on the file type
    public partial class MainWindow
    {
        void ApplyFileType(string path)
        {
            string extension = Path.GetExtension(path).ToLowerInvariant();

            DetailsTab.Header = extension switch
            {
                ".msi" or ".msp" => "Installer Details",
                ".dll" or ".rll" => "Library Information",
                ".sys" or ".ocx" => "Driver Details",
                _ => "Application Details"   // .exe and anything else
            };

            // Installer packages are not PE files, so the PE tab would be empty
            _isInstaller = extension is ".msi" or ".msp";
            _isLibrary = extension is ".dll" or ".rll";
            _isDriver = extension is ".sys" or ".ocx";
            PeTab.Visibility = VisibleIf(!_isInstaller);
            DependenciesTab.Visibility = VisibleIf(!_isInstaller);
            OnPcTab.Visibility = VisibleIf(!_isInstaller);
            InstallsTab.Visibility = VisibleIf(extension == ".msi");

            // Installers have their own sections instead of the file version ones
            VersionSection.Visibility = VisibleIf(!_isInstaller);
            InstallerSection.Visibility = VisibleIf(_isInstaller);
            LibrarySection.Visibility = VisibleIf(_isLibrary);
            DriverSection.Visibility = VisibleIf(_isDriver);
        }

        string _filePath = "";   // the file this window was opened for
        bool _isInstaller;
        bool _isLibrary;
        bool _isDriver;

        static System.Windows.Visibility VisibleIf(bool visible) =>
            visible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
    }
}
