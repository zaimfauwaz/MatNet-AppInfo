using AppInfoGUI.Strings;
using System.Windows;
using System.Windows.Controls;

namespace AppInfoGUI
{
    // The Strings tab: readable text found in the file, with a button per kind to narrow the list
    public partial class MainWindow
    {
        List<FoundString> _foundStrings = new();

        async Task LoadStringsAsync(string path)
        {
            StringsMessage.Text = "Reading...";

            try
            {
                StringsResult result = await Task.Run(() => StringScanner.Read(path));

                _foundStrings = result.Items;
                StringsMessage.Text = result.Message;
                BuildStringButtons();
                ShowStrings(null);
            }
            catch (Exception ex)
            {
                StringsMessage.Text = "Could not read the strings: " + ex.Message;
            }
        }

        // "All" first, then one button per kind that has something in it
        void BuildStringButtons()
        {
            StringButtons.Children.Clear();
            AddStringButton($"All ({_foundStrings.Count})", null, true);

            foreach (string kind in StringKinds.All)
            {
                int count = _foundStrings.Count(s => s.Kind == kind);
                if (count > 0)
                    AddStringButton($"{StringKinds.Plural(kind)} ({count})", kind, false);
            }
        }

        void AddStringButton(string text, string? kind, bool selected)
        {
            var button = new RadioButton
            {
                Content = text,
                Tag = kind,
                IsChecked = selected,
                GroupName = "StringKinds",
                Margin = new Thickness(0, 0, 8, 6),
                Padding = new Thickness(10, 3, 10, 3)
            };

            // Looks like a button when the theme provides one, otherwise stays a plain radio button
            if (TryFindResource(typeof(System.Windows.Controls.Primitives.ToggleButton)) is Style style)
                button.Style = style;

            button.Checked += (_, _) => ShowStrings((string?)button.Tag);

            StringButtons.Children.Add(button);
        }

        void ShowStrings(string? kind)
        {
            StringsGrid.ItemsSource = kind == null ? _foundStrings : _foundStrings.Where(s => s.Kind == kind).ToList();
        }
    }
}
