namespace AppInfoGUI.Library;

// What a DLL offers to other programs
public record ExportInfo(int FunctionCount, List<string> Names, bool IsDotNet);
