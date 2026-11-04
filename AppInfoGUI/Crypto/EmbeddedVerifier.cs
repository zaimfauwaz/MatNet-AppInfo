using System.Runtime.InteropServices;

namespace AppInfoGUI.Crypto;

public static class EmbeddedVerifier
{
    public static uint Verify(string path)
    {
        // 1. Tell Windows which file to check
        var file = new WinTrust.WinTrustFileInfo
        {
            Size = (uint)Marshal.SizeOf<WinTrust.WinTrustFileInfo>(),
            FilePath = path
        };
        IntPtr filePtr = Marshal.AllocHGlobal(Marshal.SizeOf<WinTrust.WinTrustFileInfo>());
        Marshal.StructureToPtr(file, filePtr, false);

        // 2. Tell Windows how to check it
        var data = new WinTrust.WinTrustData
        {
            Size = (uint)Marshal.SizeOf<WinTrust.WinTrustData>(),
            UIChoice = WinTrust.UiNone,
            RevocationChecks = WinTrust.RevokeWholeChain,
            UnionChoice = WinTrust.ChoiceFile,
            FileInfo = filePtr,
            StateAction = WinTrust.ActionVerify
        };

        try
        {
            // 3. Ask Windows, then let it clean up
            uint code = WinTrust.WinVerifyTrust(new IntPtr(-1), WinTrust.GenericVerify, ref data);

            data.StateAction = WinTrust.ActionClose;
            WinTrust.WinVerifyTrust(new IntPtr(-1), WinTrust.GenericVerify, ref data);

            // 4. Turn the number into a sentence
            return code;
        }
        finally
        {
            Marshal.DestroyStructure<WinTrust.WinTrustFileInfo>(filePtr);
            Marshal.FreeHGlobal(filePtr);
        }
    }
}