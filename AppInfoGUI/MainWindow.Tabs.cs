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
                ".dll" => "Library Information",
                ".sys" or ".ocx" => "Driver Details",
                _ => "Application Details"   // .exe and anything else
            };

            // Installer packages are not PE files, so the PE tab would be empty
            bool isInstaller = extension is ".msi" or ".msp";
            PeTab.Visibility = isInstaller ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
        }
    }
}
