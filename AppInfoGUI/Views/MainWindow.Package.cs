using AppInfoGUI.Package;
using System.Windows;

namespace AppInfoGUI
{
    // The Package Contents tab: what is inside an installer or self-extracting .exe
    public partial class MainWindow
    {
        CancellationTokenSource? _packageCancel;

        void CancelPackage_Click(object sender, RoutedEventArgs e)
        {
            _packageCancel?.Cancel();
            CancelPackageButton.IsEnabled = false;
            PackageMessage.Text = "Cancelling...";
        }

        async Task LoadPackageAsync(string path)
        {
            if (!_kind.IsProgram)
                return;

            try
            {
                // Finding the format is quick, so the tab appears at once. Listing a big installer can take a while.
                Detection detection = await Task.Run(() => PackageReader.Detect(path));
                if (detection.Format == null)
                    return;   // a plain program: the tab stays hidden

                PackageTab.Visibility = VisibleIf(true);
                PackageMessage.Text = detection.Format.Name + ". Reading the files inside, this can take a while for big installers...";

                using var cancel = new CancellationTokenSource();
                _packageCancel = cancel;
                CancelPackageButton.IsEnabled = true;
                CancelPackageButton.Visibility = VisibleIf(true);

                PackageInfo info;
                try
                {
                    info = await Task.Run(() => PackageReader.Read(path, detection, cancel.Token));
                }
                finally
                {
                    CancelPackageButton.Visibility = VisibleIf(false);
                    _packageCancel = null;
                }

                PackageMessage.Text = info.Message;
                PackageGrid.ItemsSource = info.Entries;

                // Stopped by the person: the contents were not checked, and that is never treated as fine
                if (info.Cancelled)
                    _summary.Failures.Add("package contents (cancelled)");

                _summary.PackageName = info.Format.Name;
                _summary.PackageListed = info.Listed;
                _summary.PackageScripts = info.Scripts;
                _summary.PackageDisguised = info.Disguised;
                _summary.PackageEncrypted = info.Encrypted;
            }
            catch (Exception)
            {
                _summary.Failures.Add("package contents");
            }

            UpdateSummary();
        }
    }
}
