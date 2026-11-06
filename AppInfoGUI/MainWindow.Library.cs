using AppInfoGUI.Library;
using System.IO;

namespace AppInfoGUI
{
    // The extra section for .dll files
    public partial class MainWindow
    {
        async Task LoadLibraryAsync(string path)
        {
            if (!_isLibrary || !File.Exists(path))
                return;

            try
            {
                LibraryList.ItemsSource = await Task.Run(() => LibraryReader.Read(path));
            }
            catch (Exception)
            {
                LibrarySection.Visibility = System.Windows.Visibility.Collapsed;
            }
        }
    }
}
