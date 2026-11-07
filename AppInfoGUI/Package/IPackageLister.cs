using System.IO;

namespace AppInfoGUI.Package;

// Lists the files inside a package without unpacking anything.
// To support another format: write one of these and give it to the format in PackageFormat.cs.
public interface IPackageLister
{
    // The package data is the part of the file from overlay.Start to overlay.End
    List<PackageEntry> List(Stream file, Overlay overlay);
}
