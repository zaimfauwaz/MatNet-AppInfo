namespace AppInfoGUI.Package.Nsis;

// A file the installer writes: its path, and where its data is, counted from the start of the files
public record NsisFile(string Path, int DataOffset);
