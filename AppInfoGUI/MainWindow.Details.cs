using AppInfoGUI.Info;
using System.IO;

namespace AppInfoGUI
{
    // The Application Details tab. The signature tab lives in MainWindow.xaml.cs.
    public partial class MainWindow
    {
        async Task LoadDetailsAsync(string path)
        {
            if (!File.Exists(path))
            {
                TitleText.Text = "File not found";
                return;
            }

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
            TitleText.Text = info.Title;
            ProductText.Text = info.Product;
            CompanyText.Text = info.Company;

            AppIcon.Source = info.Icon?.Image;
            IconInfoText.Text = info.Icon == null
                ? "No icon in this file"
                : $"Largest icon in file: {info.Icon.Width} x {info.Icon.Height} px (of {info.Icon.SizeCount} sizes)";

            FileList.ItemsSource = info.FileRows;
            VersionList.ItemsSource = info.VersionRows;
            OriginList.ItemsSource = info.OriginRows;
        }
    }
}
