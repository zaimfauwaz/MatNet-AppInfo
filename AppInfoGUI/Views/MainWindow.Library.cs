using AppInfoGUI.Library;

namespace AppInfoGUI
{
    // The extra section for .dll and .rll files
    public partial class MainWindow
    {
        async Task LoadLibraryAsync(string path)
        {
            if (!_kind.IsLibrary)
                return;

            try
            {
                LibraryList.ItemsSource = await Task.Run(() => LibraryReader.Read(path));
            }
            catch (Exception)
            {
                LibrarySection.Visibility = VisibleIf(false);
            }
        }
    }
}
