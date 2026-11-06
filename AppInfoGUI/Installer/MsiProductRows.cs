using AppInfoGUI.Info;
using System.Globalization;

namespace AppInfoGUI.Installer;

// Product facts from the Property table (.msi only)
internal static class MsiProductRows
{
    public static void Fill(List<DetailRow> rows, Dictionary<string, string> props)
    {
        rows.Add(new("Product name", Safe.Text(() => props["ProductName"])));
        rows.Add(new("Product version", Safe.Text(() => props["ProductVersion"])));
        rows.Add(new("Manufacturer", Safe.Text(() => props["Manufacturer"])));
        rows.Add(new("Product code", Safe.Text(() => props["ProductCode"])));
        rows.Add(new("Upgrade code", Safe.Text(() => props["UpgradeCode"])));
        rows.Add(new("Language", Safe.Text(() => CultureInfo.GetCultureInfo(int.Parse(props["ProductLanguage"])).EnglishName)));
    }
}
