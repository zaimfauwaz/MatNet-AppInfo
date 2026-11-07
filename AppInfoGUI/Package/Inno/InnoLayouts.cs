using System.Text.RegularExpressions;

namespace AppInfoGUI.Package.Inno;

// The Inno Setup format versions we can read. A new version is one more line here.
// The numbers come from the record definitions in Inno Setup's own source (Shared.Struct.pas).
public static class InnoLayouts
{
    static readonly InnoLayout[] Known =
    {
        new("5.4.2") { Header = new(26, 4, 156), File = new(10, 0, 43), LocationNumberAt = 20, LocationTail = 74, LocationSizeAt = 20, NeedsUnicodeMark = true, Icon = new(13, 0, 32) },
        new("5.5.0") { Header = new(27, 4, 160), File = new(10, 0, 43), LocationNumberAt = 20, LocationTail = 74, LocationSizeAt = 20, NeedsUnicodeMark = true, Icon = new(13, 0, 32) },
        new("5.5.6") { Header = new(28, 4, 160), File = new(10, 0, 43), LocationNumberAt = 20, LocationTail = 74, LocationSizeAt = 20, NeedsUnicodeMark = true, Icon = new(13, 0, 32) },
        new("5.5.7") { Header = new(28, 4, 157), File = new(10, 0, 43), LocationNumberAt = 20, LocationTail = 74, LocationSizeAt = 20, NeedsUnicodeMark = true, Icon = new(13, 0, 32) },
        new("6.0.0") { Header = new(30, 4, 167), File = new(10, 0, 43), LocationNumberAt = 20, LocationTail = 74, LocationSizeAt = 20, NeedsUnicodeMark = true, Icon = new(13, 0, 32) },
        new("6.1.0") { Header = new(30, 4, 167), File = new(10, 0, 43), LocationNumberAt = 20, LocationTail = 74, LocationSizeAt = 20, NeedsUnicodeMark = true },
        new("6.3.0") { Header = new(32, 4, 166), File = new(10, 0, 43), LocationNumberAt = 20, LocationTail = 75, LocationSizeAt = 20 },
        new("6.4.0.1") { Header = new(32, 4, 177), File = new(10, 0, 43), LocationNumberAt = 20, LocationTail = 87, LocationSizeAt = 20 },
        new("6.4.2") { Header = new(33, 4, 177), File = new(10, 0, 43), LocationNumberAt = 20, LocationTail = 87, LocationSizeAt = 20 },
        new("6.4.3") { Header = new(33, 4, 177), File = new(10, 0, 43), LocationNumberAt = 20, LocationTail = 85, LocationSizeAt = 20 },
        new("6.5.0") { Header = new(34, 4, 133), File = new(15, 1, 77), LocationNumberAt = 53, LocationTail = 85, LocationSizeAt = 20, HasSigningKeys = true, HasEncryptionHeader = true },
        new("6.5.2") { Header = new(34, 4, 141), File = new(15, 1, 77), LocationNumberAt = 53, LocationTail = 89, LocationSizeAt = 24, HasSigningKeys = true, HasEncryptionHeader = true },
        new("6.6.0") { Header = new(34, 4, 149), Language = new(4, 4, 19), File = new(15, 1, 77), LocationNumberAt = 53, LocationTail = 89, LocationSizeAt = 24, HasSigningKeys = true, HasEncryptionHeader = true },
        new("6.6.1") { Header = new(34, 4, 150), Language = new(4, 4, 19), File = new(15, 1, 77), LocationNumberAt = 53, LocationTail = 89, LocationSizeAt = 24, HasSigningKeys = true, HasEncryptionHeader = true },
        new("6.7.0") { Header = new(39, 4, 162), Language = new(4, 4, 19), Component = new(5, 0, 39), Task = new(6, 0, 23), File = new(15, 1, 80), LocationNumberAt = 53, LocationTail = 89, LocationSizeAt = 24, WideBlockSize = true, HasSigningKeys = true, HasEncryptionHeader = true }
    };

    // The text at the start of the setup data, like "Inno Setup Setup Data (6.4.3)"
    public static InnoLayout? Find(string setupId)
    {
        Match version = Regex.Match(setupId, @"\(([\d.]+)\)");
        if (!version.Success) return null;

        InnoLayout? layout = Known.FirstOrDefault(k => k.SetupId == version.Groups[1].Value);
        if (layout == null) return null;

        bool unicode = setupId.EndsWith("(u)");
        return layout.NeedsUnicodeMark && !unicode ? null : layout;
    }
}
