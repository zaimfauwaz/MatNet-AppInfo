using AppInfoGUI.Package.Archives;
using AppInfoGUI.Package.Inno;
using AppInfoGUI.Package.Nsis;
using AppInfoGUI.Package.Wix;

namespace AppInfoGUI.Package;

// A kind of installer or self-extracting package, and how to list what is inside (if we can).
public class PackageFormat
{
    public string Name { get; }
    public IPackageLister? Lister { get; }   // null: we can recognise it but not list it yet

    PackageFormat(string name, IPackageLister? lister = null)
    {
        Name = name;
        Lister = lister;
    }

    public static readonly PackageFormat Zip = new("Self-extracting ZIP archive", new ZipLister());
    public static readonly PackageFormat SevenZip = new("Self-extracting 7-Zip archive", ArchiveLister.SevenZip());
    public static readonly PackageFormat Rar = new("Self-extracting RAR archive", ArchiveLister.Rar());
    public static readonly PackageFormat Nsis = new("NSIS installer", new NsisLister());
    public static readonly PackageFormat InnoSetup = new("Inno Setup installer", new InnoLister());
    public static readonly PackageFormat WixBurn = new("WiX installer bundle", new WixLister());
    public static readonly PackageFormat InstallShield = new("InstallShield installer");
    public static readonly PackageFormat PyInstaller = new("Python program packed with PyInstaller");
    public static readonly PackageFormat UnknownData = new("Program with extra data attached (format not recognised)");
}
