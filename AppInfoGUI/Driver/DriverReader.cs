using System.IO;
using System.Reflection.PortableExecutable;
using AppInfoGUI.Info;
using AppInfoGUI.Library;

namespace AppInfoGUI.Driver;

public record DriverInfo(string Title, List<DetailRow> Rows);

// The extra rows for a .sys driver or an .ocx control. Nothing is loaded or run.
public static class DriverReader
{
    public static DriverInfo Read(string path)
    {
        bool isDriver = Path.GetExtension(path).Equals(".sys", StringComparison.OrdinalIgnoreCase);
        var rows = new List<DetailRow>();

        if (isDriver)
        {
            FillDriver(rows, path);
            return new DriverInfo("Driver on this PC", rows);
        }

        FillControl(rows, path);
        return new DriverInfo("ActiveX control on this PC", rows);
    }

    static void FillDriver(List<DetailRow> rows, string path)
    {
        DriverService? service = null;
        bool searched = false;
        try
        {
            service = DriverServiceFinder.Find(path);
            searched = true;
        }
        catch (Exception)
        {
            // Shown below as "could not check"
        }

        if (!searched)
            rows.Add(new("Registered as a driver", "Could not check"));
        else if (service == null)
            rows.Add(new("Registered as a driver", "No - Windows has no driver entry pointing to this file"));
        else
        {
            rows.Add(new("Registered as a driver", $"Yes - service '{service.Name}'"));
            rows.Add(new("Display name", service.DisplayName));
            rows.Add(new("Driver type", service.DriverType));
            rows.Add(new("Starts", service.StartType));
        }

        rows.Add(new("In a standard driver folder", Safe.Text(() => StandardFolder(path))));
        rows.Add(new("Standard driver model (WDM)", Safe.Text(() => Wdm(path))));
    }

    static void FillControl(List<DetailRow> rows, string path)
    {
        rows.Add(new("Can register itself", Safe.Text(() => CanRegister(path))));

        string? id = null;
        bool searched = false;
        try
        {
            id = ComRegistration.Find(path);
            searched = true;
        }
        catch (Exception)
        {
            // Shown below as "could not check"
        }

        rows.Add(new("Registered on this PC", !searched ? "Could not check"
            : id == null ? "No - Windows has no registration pointing to this file"
            : $"Yes - {id}"));
    }

    static string StandardFolder(string path)
    {
        string system = Environment.GetFolderPath(Environment.SpecialFolder.System);
        string folder = Path.GetDirectoryName(path) ?? "";

        bool standard = LocationDescriber.IsInside(folder, Path.Combine(system, "drivers"))
                     || LocationDescriber.IsInside(folder, Path.Combine(system, "DriverStore"));

        return standard ? "Yes" : "No - drivers normally live in System32\\drivers or the driver store";
    }

    // Almost every modern driver sets this flag
    static string Wdm(string path)
    {
        using var stream = File.OpenRead(path);
        using var pe = new PEReader(stream);

        ushort flags = (ushort)pe.PEHeaders.PEHeader!.DllCharacteristics;
        return (flags & 0x2000) != 0 ? "Yes" : "No";
    }

    static string CanRegister(string path)
    {
        using var stream = File.OpenRead(path);
        using var pe = new PEReader(stream);

        bool yes = ExportTable.Read(pe).Names.Contains("DllRegisterServer");
        return yes ? "Yes (has a registration function)" : "No registration function found";
    }
}
