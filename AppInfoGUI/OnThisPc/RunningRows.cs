using AppInfoGUI.Info;
using System.Diagnostics;

namespace AppInfoGUI.OnThisPc;

internal static class RunningRows
{
    public static List<DetailRow> Read(string path)
    {
        var rows = new List<DetailRow>();
        int skipped = 0;

        foreach (Process process in Process.GetProcesses())
        {
            using (process)
            {
                string? image = ProcessNative.ImagePath(process.Id);

                if (image == null) skipped++;
                else if (image.Equals(path, StringComparison.OrdinalIgnoreCase))
                    rows.Add(new(process.ProcessName, "Process ID " + process.Id));
            }
        }

        // Windows protects some of its own processes from being inspected
        if (skipped > 0)
            rows.Add(new("Not checked", $"{skipped} protected processes could not be inspected"));

        return rows;
    }
}
