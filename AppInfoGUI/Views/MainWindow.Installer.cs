using AppInfoGUI.Installer;

namespace AppInfoGUI
{
    // The details for .msi and .msp files
    public partial class MainWindow
    {
        async Task LoadInstallerAsync(string path)
        {
            if (!_kind.IsInstaller)
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

            ProductSection.Visibility = VisibleIf(info.ProductRows.Count > 0);
        }
    }
}
