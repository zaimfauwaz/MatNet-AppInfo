namespace AppInfoGUI.Strings;

public class StringsResult
{
    public List<FoundString> Items { get; } = new();

    // Anything the person should know about what was or was not read
    public string Message { get; set; } = "";
}
