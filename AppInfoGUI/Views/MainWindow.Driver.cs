using AppInfoGUI.Driver;

namespace AppInfoGUI
{
    // The extra section for .sys and .ocx files
    public partial class MainWindow
    {
        async Task LoadDriverAsync(string path)
        {
            if (!_kind.IsDriver)
                return;

            try
            {
                DriverInfo info = await Task.Run(() => DriverReader.Read(path));
                DriverTitle.Text = info.Title;
                DriverList.ItemsSource = info.Rows;

                _summary.DriverCheck = info.Check;
                UpdateSummary();
            }
            catch (Exception)
            {
                DriverSection.Visibility = VisibleIf(false);
                _summary.Failures.Add("driver details");
            }
        }
    }
}
