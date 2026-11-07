using AppInfoGUI.Summary;
using System.IO;
using System.Windows.Media;

namespace AppInfoGUI
{
    // The banner above the tabs. The tabs report what they find, SummaryBuilder decides the verdict.
    public partial class MainWindow
    {
        readonly SummaryInputs _summary = new();

        // The checks the summary needs that no tab does
        async Task LoadSummaryChecksAsync(string path)
        {
            try
            {
                FileFacts facts = await Task.Run(() => SummaryChecks.Read(path, _kind.IsInstaller));

                _summary.Downloaded = facts.Downloaded;
                _summary.RequestedLevel = facts.RequestedLevel;
                _summary.InTempFolder = facts.InTempFolder;
                _summary.FileName = Path.GetFileName(path);
                _summary.OriginalName = facts.OriginalName;
                _summary.Failures.AddRange(facts.Failures);
            }
            catch (Exception)
            {
                _summary.Failures.Add("file facts");
            }

            UpdateSummary();
        }

        void UpdateSummary()
        {
            Verdict verdict = SummaryBuilder.Build(_summary);

            Brush color = verdict.Level switch
            {
                VerdictLevel.Danger => Brushes.LightCoral,
                VerdictLevel.Review => Brushes.Khaki,
                VerdictLevel.Good => Brushes.LightGreen,
                _ => Brushes.LightGray
            };

            ShowBanner(verdict.Headline, color);
            FindingList.ItemsSource = verdict.Findings.Select(f => new FindingLine(DotColor(f.Level), f.Text)).ToList();
        }

        static Brush DotColor(FindingLevel level) => level switch
        {
            FindingLevel.Danger => Brushes.DarkRed,
            FindingLevel.Review => Brushes.DarkGoldenrod,
            _ => Brushes.Gray
        };

        // A plain message in the banner, with no findings under it
        void ShowBanner(string text, Brush color)
        {
            VerdictText.Text = text;
            VerdictBanner.Background = color;
            FindingList.ItemsSource = null;
        }
    }

    // One line under the headline: a coloured dot and the text
    public record FindingLine(Brush Dot, string Text);
}
