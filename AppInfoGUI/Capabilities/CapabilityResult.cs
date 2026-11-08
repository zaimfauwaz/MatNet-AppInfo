namespace AppInfoGUI.Capabilities;

public class CapabilityResult
{
    public List<CapabilityRow> Rows { get; } = new();
    public string Message { get; set; } = "";
}
