using AppInfoGUI.Info;

namespace AppInfoGUI.Installer;

internal static class ServiceEntries
{
    public static IEnumerable<ServiceEntry> Read(MsiPackage package)
    {
        string sql = "SELECT `Name`, `DisplayName`, `StartType`, `StartName` FROM `ServiceInstall`";

        foreach (string[] row in package.Query(sql, 4))
        {
            string display = row[1] == "" ? row[0] : row[1];
            string account = row[3] == "" ? "LocalSystem (full control of the PC)" : row[3];

            yield return new ServiceEntry(row[0], display, ServiceStartType.Describe(row[2]), account);
        }
    }
}
