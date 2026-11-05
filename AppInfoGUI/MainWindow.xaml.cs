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

            await LoadSignatureAsync(args[1]);
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
            PublisherBox.Text = r.Publisher;
            IssuerBox.Text = r.Issuer;
            ThumbprintBox.Text = r.Thumbprint;
            ValidFromBox.Text = r.ValidFrom;
            ValidToBox.Text = r.ValidTo;
            CatalogBox.Text = r.CatalogFile;
        }

        void ShowBanner(string text, Brush color)
        {
            VerdictText.Text = text;
            VerdictBanner.Background = color;
        }

        void ClearDetails()
        {
            TypeBox.Text = "";
            PublisherBox.Text = "";
            IssuerBox.Text = "";
            ThumbprintBox.Text = "";
            ValidFromBox.Text = "";
            ValidToBox.Text = "";
            CatalogBox.Text = "";
        }

    }
}