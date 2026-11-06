using AppInfoGUI.Info;

namespace AppInfoGUI.Library;

internal static class LibraryRows
{
    public static void FillExports(List<DetailRow> rows, ExportInfo exports)
    {
        rows.Add(new("Exported functions", Safe.Text(() => Count(exports))));
        rows.Add(new("Looks like", Safe.Text(() => Kind(exports))));
    }

    static string Count(ExportInfo exports)
    {
        if (exports.FunctionCount == 0)
            return exports.IsDotNet
                ? "None (a .NET library is used through its assembly instead)"
                : "None";

        return $"{exports.FunctionCount} ({exports.Names.Count} by name)";
    }

    // Some well-known function names show what a DLL is for
    static string Kind(ExportInfo exports)
    {
        var kinds = new List<string>();

        if (Has(exports, "DllGetClassObject") || Has(exports, "DllRegisterServer"))
            kinds.Add("COM component");
        if (Has(exports, "ServiceMain"))
            kinds.Add("Windows service host");
        if (Has(exports, "CPlApplet"))
            kinds.Add("Control Panel item");
        if (exports.IsDotNet)
            kinds.Add(".NET library");

        return kinds.Count == 0 ? "General library" : string.Join(", ", kinds);
    }

    static bool Has(ExportInfo exports, string name) => exports.Names.Contains(name);
}
