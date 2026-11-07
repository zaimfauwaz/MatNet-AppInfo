namespace AppInfoGUI.Package.Wix;

// A file of a bundle. "Packaging" says where it is: inside the bundle (embedded), next to it (external),
// or on a web server (download).
public record BurnPayload(string Id, string FilePath, long FileSize, string Packaging, string? DownloadUrl, string? SourcePath);

// One step of what the bundle installs: an .exe, .msi, .msp or .msu, with the file that holds it
public record BurnPackage(string Kind, string Id, long Size, BurnPayload? Payload);

// What the bundle's manifest says about the bundle
public record BurnBundle(
    string? Name,
    string? Version,
    string? Publisher,
    List<BurnPackage> Packages,
    List<BurnPayload> InterfaceFiles);
