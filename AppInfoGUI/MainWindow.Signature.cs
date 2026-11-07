using AppInfoGUI.Crypto;
using System.Windows.Media;

namespace AppInfoGUI
{
    // The Digital Signatures tab and the verdict banner
    public partial class MainWindow
    {
        async Task LoadSignatureAsync(string path)
        {
            ShowBanner("Checking...", Brushes.LightGray);

            try
            {
                SignatureResult result = await Task.Run(() => SignatureVerifier.Check(path));
                ShowResult(result);
            }
            catch (Exception ex)
            {
                ShowBanner("Could not check this file: " + ex.Message, Brushes.LightCoral);
            }
        }

        void ShowResult(SignatureResult result)
        {
            Brush color = Brushes.LightCoral;
            if (result.IsValid) color = Brushes.LightGreen;
            else if (result.SignatureType == "None") color = Brushes.Khaki;

            ShowBanner(result.Verdict, color);
            SignatureList.ItemsSource = SignatureRows.From(result);
        }

        void ShowBanner(string text, Brush color)
        {
            VerdictText.Text = text;
            VerdictBanner.Background = color;
        }
    }
}
