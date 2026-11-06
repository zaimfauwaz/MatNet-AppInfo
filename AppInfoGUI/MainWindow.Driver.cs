using AppInfoGUI.Driver;
using System.IO;

namespace AppInfoGUI
{
    // The extra section for .sys and .ocx files
    public partial class MainWindow
    {
        async Task LoadDriverAsync(string path)
        {
            if (!_isDriver || !File.Exists(path))
                return;

            try
            {
                DriverInfo info = await Task.Run(() => DriverReader.Read(path));
                DriverTitle.Text = info.Title;
                DriverList.ItemsSource = info.Rows;
            }
            catch (Exception)
            {
                DriverSection.Visibility = System.Windows.Visibility.Collapsed;
            }
        }
    }
}
