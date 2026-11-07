namespace AppInfoGUI.Package.Inno;

// How one kind of entry is stored in the setup data:
// some text strings, then a fixed-size block of numbers and flags.
public record InnoShape(int Strings, int AnsiStrings, int Tail);
