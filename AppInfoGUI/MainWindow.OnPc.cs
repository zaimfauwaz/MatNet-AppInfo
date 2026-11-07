using AppInfoGUI.OnThisPc;
using System.IO;
using System.Windows;

namespace AppInfoGUI
{
    // The "On This PC" tab
    public partial class MainWindow
    {
        async Task LoadOnPcAsync(string path)
        {
            if (_isInstaller)
                return;

            if (!File.Exists(path))
            {
                OnPcMessage.Text = "File not found";
                return;
            }

            OnPcMessage.Text = "Checking...";

            try
            {
                OnThisPcInfo info = await Task.Run(() => OnThisPcReader.Read(path));

                OnPcRunningList.ItemsSource = info.RunningRows;
                OnPcProgramList.ItemsSource = info.ProgramRows;
                OnPcAutostartList.ItemsSource = info.AutostartRows;
                OnPcFirewallList.ItemsSource = info.FirewallRows;
                OnPcMessage.Text = "Checked " + DateTime.Now.ToString("HH:mm:ss");
            }
            catch (Exception ex)
            {
                OnPcMessage.Text = "Could not check: " + ex.Message;
            }
        }

        // Checks again, for example after starting the program or installing it
        async void RefreshOnPc_Click(object sender, RoutedEventArgs e)
        {
            if (_filePath == "") return;

            RefreshOnPcButton.IsEnabled = false;
            await LoadOnPcAsync(_filePath);
            RefreshOnPcButton.IsEnabled = true;
        }
    }
}
