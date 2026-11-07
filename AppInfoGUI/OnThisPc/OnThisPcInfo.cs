using AppInfoGUI.Info;

namespace AppInfoGUI.OnThisPc;

// What Windows says about this file on this PC. Each list is one section on the tab.
public class OnThisPcInfo
{
    public List<DetailRow> RunningRows { get; } = new();
    public List<DetailRow> ProgramRows { get; } = new();
    public List<DetailRow> AutostartRows { get; } = new();
    public List<DetailRow> FirewallRows { get; } = new();
}
