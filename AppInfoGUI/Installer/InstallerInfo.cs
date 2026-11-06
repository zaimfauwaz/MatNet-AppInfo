using AppInfoGUI.Info;

namespace AppInfoGUI.Installer;

public class InstallerInfo
{
    public string Title { get; set; } = "";
    public string Product { get; set; } = "";
    public string Company { get; set; } = "";

    public List<DetailRow> ProductRows { get; } = new();
    public List<DetailRow> PackageRows { get; } = new();
}
