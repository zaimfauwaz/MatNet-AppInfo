using AppInfoGUI.Crypto;
using System.Windows;
using System.Windows.Media;
using System.IO;

namespace AppInfoGUI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string[] args = Environment.GetCommandLineArgs();

            if (args.Length < 2)
            {
                ShowBanner("No file given. Right-click a file and choose \"MatNet AppInfo\".", Brushes.LightGray);
                return;
            }

            string path = args[1];
            ApplyFileType(path);

            // Independent of each other, so a slow signature check never delays the details
            Task details = LoadDetailsAsync(path);
            Task signature = LoadSignatureAsync(path);
            Task pe = LoadPeAsync(path);
            Task installer = LoadInstallerAsync(path);
            await Task.WhenAll(details, signature, pe, installer);
        }

        async Task LoadSignatureAsync(string path)
        {
            FilePathBox.Text = path;
            ClearDetails();

            if (!File.Exists(path))
            {
                ShowBanner("File not found", Brushes.LightCoral);
                return;
            }

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

        void ShowResult(SignatureResult r)
        {
            Brush color = Brushes.LightCoral;
            if (r.IsValid) color = Brushes.LightGreen;
            else if (r.SignatureType == "None") color = Brushes.Khaki;

            ShowBanner(r.Verdict, color);

            TypeBox.Text = r.SignatureType;
            CatalogBox.Text = r.CatalogFile;
            PublisherBox.Text = r.Publisher;
            SubjectBox.Text = r.Subject;
            IssuerBox.Text = r.Issuer;
            ChainBox.Text = r.Chain;
            ThumbprintBox.Text = r.Thumbprint;
            SerialBox.Text = r.SerialNumber;
            AlgorithmBox.Text = r.Algorithm;
            ValidFromBox.Text = r.ValidFrom;
            ValidToBox.Text = r.ValidTo;
            ExpiryBox.Text = r.Expiry;
            ValidationBox.Text = r.Validation;
        }

        void ShowBanner(string text, Brush color)
        {
            VerdictText.Text = text;
            VerdictBanner.Background = color;
        }

        void ClearDetails()
        {
            TypeBox.Text = "";
            CatalogBox.Text = "";
            PublisherBox.Text = "";
            SubjectBox.Text = "";
            IssuerBox.Text = "";
            ChainBox.Text = "";
            ThumbprintBox.Text = "";
            SerialBox.Text = "";
            AlgorithmBox.Text = "";
            ValidFromBox.Text = "";
            ValidToBox.Text = "";
            ExpiryBox.Text = "";
            ValidationBox.Text = "";
        }

    }
}