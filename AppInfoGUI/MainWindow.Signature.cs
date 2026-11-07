using AppInfoGUI.Crypto;
using AppInfoGUI.Info;

namespace AppInfoGUI
{
    // The Digital Signatures tab. The verdict banner is in MainWindow.Summary.cs.
    public partial class MainWindow
    {
        async Task LoadSignatureAsync(string path)
        {
            try
            {
                SignatureResult result = await Task.Run(() => SignatureVerifier.Check(path));

                _summary.Signature = result;
                SignatureList.ItemsSource = SignatureRows.From(result);
            }
            catch (Exception ex)
            {
                SignatureList.ItemsSource = new List<DetailRow> { new("Signature check", "Could not check: " + ex.Message) };
            }

            UpdateSummary();
        }
    }
}
