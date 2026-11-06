using AppInfoGUI.Info;

namespace AppInfoGUI.PE;

public class PeInfo
{
    // Filled only when the file cannot be read as a PE file
    public string Message { get; set; } = "";

    public List<DetailRow> HeaderRows { get; } = new();
    public List<DetailRow> SecurityRows { get; } = new();
    public List<DetailRow> SectionRows { get; } = new();
    public List<DetailRow> DotNetRows { get; } = new();
}
