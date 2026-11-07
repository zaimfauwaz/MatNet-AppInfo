using AppInfoGUI.Info;

namespace AppInfoGUI.OnThisPc;

// Windows Firewall rules that name this file. The rules are only read.
internal static class FirewallRows
{
    public static List<DetailRow> Read(string path)
    {
        var rows = new List<DetailRow>();

        Type? type = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
        if (type == null) throw new InvalidOperationException("Windows Firewall is not available");

        dynamic policy = Activator.CreateInstance(type)!;

        foreach (dynamic rule in policy.Rules)
        {
            try
            {
                if (!PathMatch.Mentions((string?)rule.ApplicationName, path)) continue;

                string direction = rule.Direction == 1 ? "Incoming" : "Outgoing";
                string action = rule.Action == 1 ? "allowed" : "blocked";
                string state = rule.Enabled ? "on" : "switched off";

                rows.Add(new((string)rule.Name, $"{direction} {action} ({state})"));
            }
            catch (Exception)
            {
                // One odd rule must not stop the search
            }
        }

        return rows;
    }
}
