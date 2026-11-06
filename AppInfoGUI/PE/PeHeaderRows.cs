using AppInfoGUI.Info;
using System.Reflection.PortableExecutable;

namespace AppInfoGUI.PE;

internal static class PeHeaderRows
{
    public static void Fill(List<DetailRow> rows, PEHeaders headers)
    {
        PEHeader optional = headers.PEHeader!;
        bool is64 = optional.Magic == PEMagic.PE32Plus;

        rows.Add(new("Architecture", Safe.Text(() => Architecture(headers.CoffHeader.Machine))));
        rows.Add(new("Bits", is64 ? "64-bit (PE32+)" : "32-bit (PE32)"));
        rows.Add(new("Kind", Safe.Text(() => headers.IsDll ? "DLL (library)" : "EXE (program)")));
        rows.Add(new("Subsystem", Safe.Text(() => Subsystem(optional.Subsystem))));
        rows.Add(new("Compiled", Safe.Text(() => CompileTime(headers.CoffHeader.TimeDateStamp))));
        rows.Add(new("Windows version needed", Safe.Text(() => $"{optional.MajorSubsystemVersion}.{optional.MinorSubsystemVersion}")));
        rows.Add(new("Linker version", Safe.Text(() => $"{optional.MajorLinkerVersion}.{optional.MinorLinkerVersion}")));
        rows.Add(new("Entry point", Safe.Text(() => $"0x{optional.AddressOfEntryPoint:X}")));
        rows.Add(new("Image base", Safe.Text(() => $"0x{optional.ImageBase:X}")));
        rows.Add(new("Image size", Safe.Text(() => TextFormat.Size(optional.SizeOfImage))));
    }

    static string Architecture(Machine machine) => machine switch
    {
        Machine.I386 => "x86 (Intel/AMD 32-bit)",
        Machine.Amd64 => "x64 (Intel/AMD 64-bit)",
        Machine.Arm64 => "ARM64",
        Machine.Arm or Machine.ArmThumb2 => "ARM (32-bit)",
        _ => $"Other ({machine})"
    };

    static string Subsystem(Subsystem subsystem) => subsystem switch
    {
        System.Reflection.PortableExecutable.Subsystem.WindowsGui => "Windows GUI (has windows)",
        System.Reflection.PortableExecutable.Subsystem.WindowsCui => "Windows console (text mode)",
        System.Reflection.PortableExecutable.Subsystem.Native => "Native (driver or system component)",
        _ => subsystem.ToString()
    };

    // The compile time is written by the builder, so the file can state anything
    static string CompileTime(int stamp)
    {
        if (stamp == 0)
            return "Not set (the builder left it empty)";

        DateTime time = DateTimeOffset.FromUnixTimeSeconds((uint)stamp).UtcDateTime;
        string text = time.ToString("yyyy-MM-dd HH:mm") + " UTC (as stated by the file)";

        return time > DateTime.UtcNow ? text + " - the date is in the future (some builds store a random value here)" : text;
    }
}
