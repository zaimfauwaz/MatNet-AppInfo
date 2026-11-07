using AppInfoGUI.Info;

namespace AppInfoGUI.OnThisPc;

// What Windows says about this file on this PC. Each list is one section on the tab.
public class OnThisPcInfo
{
    public List<DetailRow> RunningRows { get; } = new();
    public List<DetailRow> ProgramRows { get; } = new();
    public List<DetailRow> AutostartRows { get; } = new();
    public List<DetailRow> FirewallRows { get; } = new();

    // How many run keys, services and scheduled tasks start this file
    public int AutostartHits => AutostartRows.Count(r =>
        r.Label.StartsWith("Run key") || r.Label.StartsWith("Service:") || r.Label.StartsWith("Scheduled task:"));

    // True when at least one place could not be checked
    public bool AutostartFailed => AutostartRows.Any(r => r.Label == "Could not check" || r.Value.StartsWith("Could not check"));
}
