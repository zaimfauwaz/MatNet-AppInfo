using AppInfoGUI.Startup;
using System.Diagnostics;
using System.Windows;

namespace AppInfoGUI
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // The "(administrator)" right-click entry asks for --admin. A debugger would be lost
            // in the second copy, so it is skipped while debugging.
            if (Elevation.WantsAdmin(e.Args) && !Elevation.IsElevated
                && !Debugger.IsAttached && Elevation.TryRelaunch(e.Args))
            {
                Shutdown();
                return;
            }

            base.OnStartup(e);
        }
    }
}
