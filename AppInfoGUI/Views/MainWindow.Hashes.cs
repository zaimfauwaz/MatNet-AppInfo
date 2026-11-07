using AppInfoGUI.Hashes;

namespace AppInfoGUI
{
    // The Hashes tab
    public partial class MainWindow
    {
        async Task LoadHashesAsync(string path)
        {
            HashMessage.Text = "Calculating...";
            HashList.ItemsSource = await Task.Run(() => HashReader.Read(path));
            HashMessage.Text = "";
        }
    }
}
