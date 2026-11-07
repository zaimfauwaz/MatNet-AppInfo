namespace AppInfoGUI.Dependencies;

// One DLL that the file asks Windows to load when it starts
public record DependencyRow(string Name, int FunctionCount, string Location, string Signed, List<string> Functions);

public class DependencyInfo
{
    public string Message { get; set; } = "";
    public List<DependencyRow> Rows { get; } = new();
}
