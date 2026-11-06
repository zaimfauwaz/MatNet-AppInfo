using System.IO;
using System.Reflection.PortableExecutable;
using AppInfoGUI.Info;

namespace AppInfoGUI.Library;

// The extra rows for a .dll. Nothing is loaded or run.
public static class LibraryReader
{
    public static List<DetailRow> Read(string path)
    {
        var rows = new List<DetailRow>();

        try
        {
            using var stream = File.OpenRead(path);
            using var pe = new PEReader(stream);
            LibraryRows.FillExports(rows, ExportTable.Read(pe));
        }
        catch (Exception)
        {
            rows.Add(new("Exported functions", "Could not read the export list"));
        }

        rows.Add(new("System file name", Safe.Text(() => SystemNameCheck.Check(path))));
        return rows;
    }
}
