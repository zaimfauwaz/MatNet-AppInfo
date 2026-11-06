using AppInfoGUI.Hashes;
using System.IO;

namespace AppInfoGUI
{
    // The Hashes tab
    public partial class MainWindow
    {
        async Task LoadHashesAsync(string path)
        {
            if (!File.Exists(path))
            {
                HashMessage.Text = "File not found";
                return;
            }

            HashMessage.Text = "Calculating...";
            HashList.ItemsSource = await Task.Run(() => HashReader.Read(path));
            HashMessage.Text = "";
        }
    }
}
