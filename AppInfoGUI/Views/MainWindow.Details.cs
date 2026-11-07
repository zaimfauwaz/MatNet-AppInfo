using AppInfoGUI.Info;

namespace AppInfoGUI
{
    // The Application Details tab. The signature tab lives in MainWindow.Signature.cs.
    public partial class MainWindow
    {
        async Task LoadDetailsAsync(string path)
        {
            try
            {
                ApplicationInfo info = await Task.Run(() => ApplicationDetails.Read(path));
                ShowDetails(info);
            }
            catch (Exception ex)
            {
                TitleText.Text = "Could not read details: " + ex.Message;
            }
        }

        void ShowDetails(ApplicationInfo info)
        {
            // For installers the names come from the package instead (MainWindow.Installer.cs)
            if (!_kind.IsInstaller)
            {
                TitleText.Text = info.Title;
                ProductText.Text = info.Product;
                CompanyText.Text = info.Company;
            }

            AppIcon.Source = info.Icon?.Image;
            IconInfoText.Text = info.Icon?.Description ?? "No icon available";

            FileList.ItemsSource = info.FileRows;
            VersionList.ItemsSource = info.VersionRows;
            OriginList.ItemsSource = info.OriginRows;
        }
    }
}
