namespace AppInfoGUI.Strings;

// One piece of readable text found in the file
public record FoundString(string Kind, string Text, long Offset)
{
    public string Position => $"0x{Offset:X8}";
}
