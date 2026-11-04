using AppInfoGUI.Crypto;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AppInfoGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            /*
            string[] args = Environment.GetCommandLineArgs();
            string path = args.Length > 1 ? args[1] : "";

            if (path == "")
                MessageBox.Show("Pass a file path as an argument.");
            else
                MessageBox.Show($"{path}\n\n{SignatureVerifier.Verify(path)}");
            */
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}