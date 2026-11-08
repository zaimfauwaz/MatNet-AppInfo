using System.Runtime.InteropServices;

namespace AppInfoGUI.Crypto;

public static class EmbeddedVerifier
{
    public const uint NoSignature = 0x800B0100;

    // Asks Windows about the file's signature
    public static uint Verify(string path) => Ask(path, 0, 0, out _);

    // Asks Windows about one signature of a file that has several (0 is the first)
    public static uint VerifyOne(string path, uint index) => Ask(path, WinTrust.VerifySpecific, index, out _);

    // How many signatures the file has, as Windows counts them
    public static int CountSignatures(string path)
    {
        uint code = Ask(path, WinTrust.GetSecondaryCount, 0, out uint secondary);
        return code == NoSignature ? 0 : 1 + (int)secondary;
    }

    static uint Ask(string path, uint flags, uint index, out uint secondary)
    {
        secondary = 0;

        // 1. Tell Windows which file to check
        var file = new WinTrust.WinTrustFileInfo
        {
            Size = (uint)Marshal.SizeOf<WinTrust.WinTrustFileInfo>(),
            FilePath = path
        };
        IntPtr filePtr = Marshal.AllocHGlobal(Marshal.SizeOf<WinTrust.WinTrustFileInfo>());
        Marshal.StructureToPtr(file, filePtr, false);

        // 2. Which signature, when a choice was made
        IntPtr settingsPtr = IntPtr.Zero;
        if (flags != 0)
        {
            var settings = new WinTrust.WinTrustSignatureSettings
            {
                Size = (uint)Marshal.SizeOf<WinTrust.WinTrustSignatureSettings>(),
                Flags = flags,
                Index = index
            };
            settingsPtr = Marshal.AllocHGlobal(Marshal.SizeOf<WinTrust.WinTrustSignatureSettings>());
            Marshal.StructureToPtr(settings, settingsPtr, false);
        }

        // 3. Tell Windows how to check it
        var data = new WinTrust.WinTrustData
        {
            Size = (uint)Marshal.SizeOf<WinTrust.WinTrustData>(),
            UIChoice = WinTrust.UiNone,
            RevocationChecks = WinTrust.RevokeWholeChain,
            UnionChoice = WinTrust.ChoiceFile,
            FileInfo = filePtr,
            StateAction = WinTrust.ActionVerify,
            SignatureSettings = settingsPtr
        };

        try
        {
            // 4. Ask Windows, then let it clean up
            uint code = WinTrust.WinVerifyTrust(new IntPtr(-1), WinTrust.GenericVerify, ref data);

            if (settingsPtr != IntPtr.Zero)
                secondary = Marshal.PtrToStructure<WinTrust.WinTrustSignatureSettings>(settingsPtr).SecondaryCount;

            data.StateAction = WinTrust.ActionClose;
            WinTrust.WinVerifyTrust(new IntPtr(-1), WinTrust.GenericVerify, ref data);

            return code;
        }
        finally
        {
            Marshal.DestroyStructure<WinTrust.WinTrustFileInfo>(filePtr);
            Marshal.FreeHGlobal(filePtr);
            if (settingsPtr != IntPtr.Zero)
                Marshal.FreeHGlobal(settingsPtr);
        }
    }
}
