using AppInfoGUI.Installer;
using System.IO;

namespace AppInfoGUI
{
    // The details for .msi and .msp files
    public partial class MainWindow
    {
        async Task LoadInstallerAsync(string path)
        {
            if (!_isInstaller || !File.Exists(path))
                return;

            try
            {
                InstallerInfo info = await Task.Run(() => InstallerReader.Read(path));
                ShowInstaller(info);
            }
            catch (Exception ex)
            {
                ProductText.Text = "Could not read the installer: " + ex.Message;
            }
        }

        void ShowInstaller(InstallerInfo info)
        {
            TitleText.Text = info.Title;
            ProductText.Text = info.Product;
            CompanyText.Text = info.Company;

            ProductList.ItemsSource = info.ProductRows;
            PackageList.ItemsSource = info.PackageRows;

            ProductSection.Visibility = info.ProductRows.Count > 0
                ? System.Windows.Visibility.Visible
                : System.Windows.Visibility.Collapsed;
        }
    }
}
