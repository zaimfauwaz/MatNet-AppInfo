using AppInfoGUI.Dependencies;
using System.IO;
using System.Reflection.PortableExecutable;

namespace AppInfoGUI.Capabilities;

// Lists which documented functions the file imports. It reads names only. Nothing is run.
public static class CapabilityReader
{
    public static CapabilityResult Read(string path)
    {
        var result = new CapabilityResult();

        using FileStream stream = File.OpenRead(path);
        using var pe = new PEReader(stream);

        if (pe.PEHeaders.PEHeader == null)
        {
            result.Message = "Not a Windows executable, so there is no import list.";
            return result;
        }

        List<ImportTable.Import> imports = ImportTable.Read(pe);

        int total = 0;
        int byNumber = 0;

        foreach (ImportTable.Import import in imports)
            foreach (string function in import.Functions)
            {
                total++;

                if (function.StartsWith("Ordinal #"))
                    byNumber++;
                else if (CapabilityTable.Find(function) is Capability known)
                    result.Rows.Add(new CapabilityRow(known.Group, known.Function, known.Does, import.Dll, known.Source));
            }

        result.Rows.Sort((a, b) => string.CompareOrdinal(a.Group + a.Function, b.Group + b.Function));
        result.Message = Describe(total, result.Rows.Count, byNumber);
        return result;
    }

    static string Describe(int total, int listed, int byNumber)
    {
        string message = total == 0
            ? "This file imports no functions."
            : $"{listed} of the {total} imported functions are in the documented list below.";

        if (byNumber == 1)
            message += " 1 is imported by number only, so it cannot be named.";
        else if (byNumber > 1)
            message += $" {byNumber} are imported by number only, so they cannot be named.";

        return message;
    }
}
