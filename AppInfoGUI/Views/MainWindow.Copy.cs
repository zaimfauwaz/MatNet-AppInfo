using AppInfoGUI.Info;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AppInfoGUI
{
    // "Copy row" on the right-click menus, for the lists and for the label/value rows
    public partial class MainWindow
    {
        void AttachCopyMenus()
        {
            foreach (DataGrid grid in new[] { PackageGrid, FilesGrid, RegistryGrid, ServicesGrid, ActionsGrid, DependencyGrid })
            {
                var copy = new MenuItem { Header = "Copy row" };
                copy.Click += (_, _) => ApplicationCommands.Copy.Execute(null, grid);   // the grid's own copy: cells separated by tabs

                grid.ContextMenu = new ContextMenu { Items = { copy } };
                grid.PreviewMouseRightButtonDown += SelectRowUnderMouse;
            }
        }

        // A right-click does not select the row by itself, so the menu would copy the wrong one
        static void SelectRowUnderMouse(object sender, MouseButtonEventArgs e)
        {
            var grid = (DataGrid)sender;

            DependencyObject? element = e.OriginalSource as DependencyObject;
            while (element != null && element is not DataGridRow)
                element = element is Visual or System.Windows.Media.Media3D.Visual3D
                    ? VisualTreeHelper.GetParent(element)
                    : LogicalTreeHelper.GetParent(element);

            if (element is DataGridRow row)
                grid.SelectedItem = row.Item;
        }

        void CopyDetailRow_Click(object sender, RoutedEventArgs e)
        {
            if (RowFromMenu(sender) is DetailRow row)
                CopyText($"{row.Label}: {row.Value}");
        }

        void CopyDetailValue_Click(object sender, RoutedEventArgs e)
        {
            if (RowFromMenu(sender) is DetailRow row)
                CopyText(row.Value);
        }

        // The row the menu was opened on
        static DetailRow? RowFromMenu(object sender)
        {
            var menu = (sender as MenuItem)?.Parent as ContextMenu;
            return (menu?.PlacementTarget as FrameworkElement)?.DataContext as DetailRow;
        }

        // Another program can be holding the clipboard for a moment. Then nothing is copied.
        static void CopyText(string text)
        {
            try
            {
                Clipboard.SetText(text);
            }
            catch (COMException)
            {
            }
        }
    }
}
