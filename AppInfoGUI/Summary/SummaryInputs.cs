using AppInfoGUI.Crypto;
using AppInfoGUI.Driver;

namespace AppInfoGUI.Summary;

// Everything the tabs have found so far. The tabs fill it in as their checks finish,
// and SummaryBuilder turns it into the verdict. Empty means "not reported yet".
public class SummaryInputs
{
    public bool Finished { get; set; }                  // true when every check has reported

    public SignatureResult? Signature { get; set; }
    public bool? Downloaded { get; set; }               // marked as downloaded from the internet
    public string? RequestedLevel { get; set; }         // from the manifest, for example "requireAdministrator"
    public bool InTempFolder { get; set; }
    public int AutostartHits { get; set; }              // run keys, services and tasks that start this file
    public int SystemActions { get; set; }              // installer actions that run code as the system
    public string? PackageName { get; set; }            // set when the file is an installer or self-extracting archive
    public bool PackageListed { get; set; }
    public int PackageScripts { get; set; }
    public int PackageDisguised { get; set; }
    public int PackageEncrypted { get; set; }
    public DriverCheckOutcome DriverCheck { get; set; } = DriverCheckOutcome.NotApplicable;

    // Names of checks that could not run
    public List<string> Failures { get; } = new();
}
