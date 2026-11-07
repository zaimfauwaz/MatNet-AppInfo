using AppInfoGUI.Dependencies;
using System.Windows;
using System.Windows.Controls;

namespace AppInfoGUI
{
    // The Dependencies tab
    public partial class MainWindow
    {
        async Task LoadDependenciesAsync(string path)
        {
            if (_kind.IsInstaller)
                return;

            try
            {
                DependencyInfo info = await Task.Run(() => DependencyReader.Read(path));
                DependencyMessage.Text = info.Message;
                DependencyGrid.ItemsSource = info.Rows;
            }
            catch (Exception ex)
            {
                DependencyMessage.Text = "Could not read the imports: " + ex.Message;
            }
        }

        // Reads the file and checks where each DLL is found again, for example after installing something
        async void RefreshDependencies_Click(object sender, RoutedEventArgs e)
        {
            if (_filePath == "")
            {
                DependencyMessage.Text = "No file selected";
                return;
            }

            RefreshDependenciesButton.IsEnabled = false;
            DependencyMessage.Text = "Refreshing...";
            DependencyGrid.ItemsSource = null;
            FunctionList.ItemsSource = null;
            FunctionTitle.Text = "Select a DLL to see its functions";

            await LoadDependenciesAsync(_filePath);

            RefreshDependenciesButton.IsEnabled = true;
        }

        // Shows the function names for the DLL that is selected in the table
        void DependencyGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DependencyGrid.SelectedItem is DependencyRow row)
            {
                FunctionTitle.Text = $"Functions imported from {row.Name}";
                FunctionList.ItemsSource = row.Functions;
            }
        }
    }
}
