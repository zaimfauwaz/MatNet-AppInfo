using AppInfoGUI.Info;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace AppInfoGUI.PE;

internal static class DotNetRows
{
    public static void Fill(List<DetailRow> rows, PEReader pe)
    {
        if (!pe.HasMetadata)
        {
            rows.Add(new("Type", "Native code (not .NET)"));
            return;
        }

        try
        {
            MetadataReader metadata = pe.GetMetadataReader();
            rows.Add(new("Type", ".NET (managed code)"));
            rows.Add(new("Runtime version", metadata.MetadataVersion));

            if (metadata.IsAssembly)
            {
                AssemblyDefinition assembly = metadata.GetAssemblyDefinition();
                rows.Add(new("Assembly name", metadata.GetString(assembly.Name)));
                rows.Add(new("Assembly version", assembly.Version.ToString()));
            }
        }
        catch (Exception)
        {
            rows.Add(new("Type", ".NET data could not be read"));
        }
    }
}
