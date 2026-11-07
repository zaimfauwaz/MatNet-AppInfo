using AppInfoGUI.Hashes;
using AppInfoGUI.PE;

namespace AppInfoGUI
{
    // The PE Section tab
    public partial class MainWindow
    {
        async Task LoadPeAsync(string path)
        {
            try
            {
                PeInfo info = await Task.Run(() => PeDetails.Read(path));
                ShowPe(info);

                // Shown after the rest, because it has to read the whole file
                if (info.Message == "")
                {
                    PeHashList.ItemsSource = await Task.Run(() => AuthenticodeHash.Read(path));
                    PeHashSection.Visibility = VisibleIf(true);
                }
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
            PeManifestList.ItemsSource = info.ManifestRows;
            PeDotNetList.ItemsSource = info.DotNetRows;
        }
    }
}
