namespace AppInfoGUI.Info;

public class ApplicationInfo
{
    public string Title { get; set; } = "";
    public string Product { get; set; } = "";
    public string Company { get; set; } = "";
    public IconImage? Icon { get; set; }

    public List<DetailRow> FileRows { get; } = new();
    public List<DetailRow> VersionRows { get; } = new();
    public List<DetailRow> OriginRows { get; } = new();
}
