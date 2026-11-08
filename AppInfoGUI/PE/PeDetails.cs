using AppInfoGUI.Info;
using System.IO;
using System.Reflection.PortableExecutable;

namespace AppInfoGUI.PE;

// Reads the PE headers. Nothing is executed and no code is inspected.
public static class PeDetails
{
    public static PeInfo Read(string path)
    {
        var info = new PeInfo();

        try
        {
            using var stream = File.OpenRead(path);
            using var pe = new PEReader(stream);

            PEHeaders headers = pe.PEHeaders;
            if (headers.PEHeader == null)
            {
                info.Message = "Not a Windows executable (no PE header).";
                return info;
            }

            PeHeaderRows.Fill(info.HeaderRows, headers);
            PeSecurityRows.Fill(info.SecurityRows, headers.PEHeader);
            FillBuild(info, pe);
            info.PackedSections.AddRange(PeSectionRows.Fill(info.SectionRows, stream, headers));
            DotNetRows.Fill(info.DotNetRows, pe);
            ManifestRows.Fill(info.ManifestRows, path);
        }
        catch (BadImageFormatException)
        {
            info.Message = "Not a Windows executable (no PE header).";
        }
        catch (Exception ex)
        {
            info.Message = "Could not read the PE data: " + ex.Message;
        }

        return info;
    }

    // Optional part: if the debug data is damaged, say so in its own section and keep the rest
    static void FillBuild(PeInfo info, PEReader pe)
    {
        try
        {
            PeBuildRows.Fill(info.BuildRows, pe);
        }
        catch (Exception)
        {
            info.BuildRows.Clear();
            info.BuildRows.Add(new("Debug information", "Not available"));
        }
    }
}
