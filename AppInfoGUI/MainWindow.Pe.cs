using AppInfoGUI.PE;
using System.IO;

namespace AppInfoGUI
{
    // The PE Section tab
    public partial class MainWindow
    {
        async Task LoadPeAsync(string path)
        {
            if (!File.Exists(path))
            {
                PeMessage.Text = "File not found";
                return;
            }

            try
            {
                PeInfo info = await Task.Run(() => PeDetails.Read(path));
                ShowPe(info);
            }
            catch (Exception ex)
            {
                PeMessage.Text = "Could not read the PE data: " + ex.Message;
            }
        }

        void ShowPe(PeInfo info)
        {
            PeMessage.Text = info.Message;

            PeHeaderList.ItemsSource = info.HeaderRows;
            PeSecurityList.ItemsSource = info.SecurityRows;
            PeSectionList.ItemsSource = info.SectionRows;
            PeDotNetList.ItemsSource = info.DotNetRows;
        }
    }
}
