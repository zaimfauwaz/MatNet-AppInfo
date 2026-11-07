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
                PackageInfo? info = await Task.Run(() => PackageReader.Read(path));
                if (info == null)
                    return;   // a plain program: the tab stays hidden

                PackageTab.Visibility = VisibleIf(true);
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
