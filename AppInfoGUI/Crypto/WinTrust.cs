using System.Runtime.InteropServices;

namespace AppInfoGUI.Crypto;

internal static class WinTrust
{
    public const uint UiNone = 2;
    public const uint RevokeWholeChain = 1;
    public const uint ChoiceFile = 1;
    public const uint ActionVerify = 1;
    public const uint ActionClose = 2;

    public static readonly Guid GenericVerify = new("00AAC56B-CD44-11d0-8CC2-00C04FC295EE");

    [DllImport("wintrust.dll", ExactSpelling = true)]
    public static extern uint WinVerifyTrust(
        IntPtr hwnd,
        [MarshalAs(UnmanagedType.LPStruct)] Guid action,
        ref WinTrustData data);

    [StructLayout(LayoutKind.Sequential)]
    public struct WinTrustFileInfo
    {
        public uint Size;
        [MarshalAs(UnmanagedType.LPWStr)] public string FilePath;
        public IntPtr FileHandle;
        public IntPtr KnownSubject;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WinTrustData
    {
        public uint Size;
        public IntPtr PolicyCallbackData;
        public IntPtr SipClientData;
        public uint UIChoice;
        public uint RevocationChecks;
        public uint UnionChoice;
        public IntPtr FileInfo;
        public uint StateAction;
        public IntPtr StateData;
        public IntPtr UrlReference;
        public uint ProvFlags;
        public uint UIContext;
        public IntPtr SignatureSettings;
    }
}