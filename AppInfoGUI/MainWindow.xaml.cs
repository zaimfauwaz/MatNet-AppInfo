using AppInfoGUI.Info;
using AppInfoGUI.Startup;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace AppInfoGUI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            AttachCopyMenus();
        }

        async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Elevation.IsElevated)
                Title += " (administrator)";

            string[] args = Environment.GetCommandLineArgs();

            if (args.Length < 2)
            {
                ShowBanner("No file given. Right-click a file and choose \"MatNet AppInfo\".", Brushes.LightGray);
                return;
            }

            string path = args[1];
            _filePath = path;
            _kind = new FileKind(path);
            FilePathBox.Text = path;
            ApplyFileType();

            // Checked once here, so no tab has to check again
            if (!File.Exists(path))
            {
                TitleText.Text = "File not found";
                ShowBanner("File not found", Brushes.LightCoral);
                return;
            }

            ShowBanner("Checking...", Brushes.LightGray);

            // Independent of each other, so a slow signature check never delays the details
            await Task.WhenAll(
                LoadSummaryChecksAsync(path),
                LoadDetailsAsync(path),
                LoadSignatureAsync(path),
                LoadPeAsync(path),
                LoadInstallerAsync(path),
                LoadLibraryAsync(path),
                LoadDriverAsync(path),
                LoadHashesAsync(path),
                LoadDependenciesAsync(path),
                LoadContentsAsync(path),
                LoadPackageAsync(path),
                LoadOnPcAsync(path));

            // Every check has reported, so the verdict can be final
            _summary.Finished = true;
            UpdateSummary();
        }
    }
}
