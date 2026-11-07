namespace AppInfoGUI.Package.Nsis;

// The unpacked script header, and where the installer's files begin in the program file.
// FilesStart is null when the whole installer is one packed stream, because then the files cannot be reached directly.
public record NsisHeaderData(byte[] Bytes, long? FilesStart);
