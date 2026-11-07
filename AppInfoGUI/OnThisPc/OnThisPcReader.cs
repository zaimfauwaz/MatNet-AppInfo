using AppInfoGUI.Info;

namespace AppInfoGUI.OnThisPc;

// Looks for this file in Windows' own records. Nothing is changed and nothing is run.
public static class OnThisPcReader
{
    public static OnThisPcInfo Read(string path)
    {
        string fullPath = System.IO.Path.GetFullPath(path);
        var info = new OnThisPcInfo();

        Fill(info.RunningRows, () => RunningRows.Read(fullPath));
        Fill(info.ProgramRows, () => ProgramRows.Read(fullPath));
        Fill(info.AutostartRows, () => AutostartRows.Read(fullPath));
        Fill(info.FirewallRows, () => FirewallRows.Read(fullPath));

        return info;
    }

    // Each section is checked on its own, and an empty one says so plainly
    static void Fill(List<DetailRow> rows, Func<List<DetailRow>> check)
    {
        try
        {
            List<DetailRow> found = check();
            rows.AddRange(found);

            if (found.All(r => r.Label == "Not checked"))
                rows.Insert(0, new("Result", "None found"));
        }
        catch (Exception ex)
        {
            rows.Add(new("Could not check", ex.Message));
        }
    }
}
