using AppInfoGUI.Package;

namespace AppInfoGUI
{
    // The Package Contents tab: what is inside an installer or self-extracting .exe
    public partial class MainWindow
    {
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

                PackageInfo info = await Task.Run(() => PackageReader.Read(path, detection));
                PackageMessage.Text = info.Message;
                PackageGrid.ItemsSource = info.Entries;

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
