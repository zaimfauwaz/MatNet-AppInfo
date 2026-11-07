namespace AppInfoGUI.Package.Nsis;

// What was found out about one file of the installer: its size, and the rows of the archive it holds, if it is one
public record NsisFileInfo(long Size, List<PackageEntry> Nested);
