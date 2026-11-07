using System.IO;
using System.Reflection.PortableExecutable;

namespace AppInfoGUI.Dependencies;

// The table for the Dependencies tab. Nothing is loaded or run.
public static class DependencyReader
{
    public static DependencyInfo Read(string path)
    {
        var info = new DependencyInfo();

        try
        {
            using var stream = File.OpenRead(path);
            using var pe = new PEReader(stream);

            if (pe.PEHeaders.PEHeader == null)
            {
                info.Message = "Not a Windows executable (no PE header).";
                return info;
            }

            bool is32Bit = pe.PEHeaders.PEHeader.Magic == PEMagic.PE32;
            string folder = Path.GetDirectoryName(path) ?? "";

            foreach (var import in ImportTable.Read(pe))
            {
                string where = DependencyLocator.Where(import.Dll, folder, is32Bit);

                // Windows' own files are trusted through Windows itself, so only the program's own DLLs are checked
                string signed = where == DependencyLocator.SameFolder
                    ? DependencySignature.Check(Path.Combine(folder, import.Dll))
                    : DependencySignature.NotChecked;

                info.Rows.Add(new DependencyRow(import.Dll, import.Functions.Count, where, signed, import.Functions));
            }

            if (info.Rows.Count == 0)
                info.Message = "This file does not import any DLLs.";
            else if (pe.HasMetadata)
                info.Message = "This is a .NET file. Its own .NET references are not listed here.";
        }
        catch (BadImageFormatException)
        {
            info.Message = "Not a Windows executable (no PE header).";
        }
        catch (Exception ex)
        {
            info.Message = "Could not read the imports: " + ex.Message;
        }

        return info;
    }
}
