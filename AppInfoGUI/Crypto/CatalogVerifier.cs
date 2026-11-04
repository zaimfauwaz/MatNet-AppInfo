using System.Runtime.InteropServices;
using System.IO;

namespace AppInfoGUI.Crypto;

public static class CatalogVerifier
{
    const uint NoSignature = 0x800B0100;

    public static uint Verify(string path)
    {
        // Catalogs use either SHA-256 or SHA-1, so try both
        foreach (string algorithm in new[] { "SHA256", "SHA1" })
        {
            uint code = VerifyWith(path, algorithm);
            if (code != NoSignature) return code;
        }
        return NoSignature;
    }

    static uint VerifyWith(string path, string algorithm)
    {
        using var file = File.OpenRead(path);
        IntPtr fileHandle = file.SafeFileHandle.DangerousGetHandle();

        // 1. Open the catalog service
        if (!WinCatalog.CryptCATAdminAcquireContext2(out IntPtr catAdmin, IntPtr.Zero, algorithm, IntPtr.Zero, 0))
            return NoSignature;

        try
        {
            // 2. Compute the file's catalog hash (ask for the size first, then fill it)
            uint hashSize = 0;
            WinCatalog.CryptCATAdminCalcHashFromFileHandle2(catAdmin, fileHandle, ref hashSize, null, 0);
            byte[] hash = new byte[hashSize];
            if (!WinCatalog.CryptCATAdminCalcHashFromFileHandle2(catAdmin, fileHandle, ref hashSize, hash, 0))
                return NoSignature;

            // 3. Find a catalog that lists this hash
            IntPtr catInfo = WinCatalog.CryptCATAdminEnumCatalogFromHash(catAdmin, hash, hashSize, 0, IntPtr.Zero);
            if (catInfo == IntPtr.Zero)
                return NoSignature;

            try
            {
                // 4. Get the catalog's file path
                var info = new WinCatalog.CatalogInfo { Size = (uint)Marshal.SizeOf<WinCatalog.CatalogInfo>() };
                if (!WinCatalog.CryptCATCatalogInfoFromContext(catInfo, ref info, 0))
                    return NoSignature;

                // 5. Ask Windows to verify the file against that catalog
                return CheckAgainstCatalog(path, hash, info.CatalogFile, catAdmin);
            }
            finally
            {
                WinCatalog.CryptCATAdminReleaseCatalogContext(catAdmin, catInfo, 0);
            }
        }
        finally
        {
            WinCatalog.CryptCATAdminReleaseContext(catAdmin, 0);
        }
    }

    static uint CheckAgainstCatalog(string path, byte[] hash, string catalogFile, IntPtr catAdmin)
    {
        // Windows wants the hash in unmanaged memory
        IntPtr hashPtr = Marshal.AllocHGlobal(hash.Length);
        Marshal.Copy(hash, 0, hashPtr, hash.Length);

        var catalog = new WinCatalog.WinTrustCatalogInfo
        {
            Size = (uint)Marshal.SizeOf<WinCatalog.WinTrustCatalogInfo>(),
            CatalogFilePath = catalogFile,
            MemberTag = Convert.ToHexString(hash),   // the hash as hex text
            MemberFilePath = path,
            CalculatedFileHash = hashPtr,
            CalculatedFileHashSize = (uint)hash.Length,
            CatAdmin = catAdmin
        };
        IntPtr catalogPtr = Marshal.AllocHGlobal(Marshal.SizeOf<WinCatalog.WinTrustCatalogInfo>());
        Marshal.StructureToPtr(catalog, catalogPtr, false);

        var data = new WinTrust.WinTrustData
        {
            Size = (uint)Marshal.SizeOf<WinTrust.WinTrustData>(),
            UIChoice = WinTrust.UiNone,
            RevocationChecks = WinTrust.RevokeWholeChain,
            UnionChoice = WinCatalog.ChoiceCatalog,  // "we are checking against a catalog"
            FileInfo = catalogPtr,                   // this field is a union pointer, so it holds the catalog struct here
            StateAction = WinTrust.ActionVerify
        };

        try
        {
            uint code = WinTrust.WinVerifyTrust(new IntPtr(-1), WinTrust.GenericVerify, ref data);

            data.StateAction = WinTrust.ActionClose;
            WinTrust.WinVerifyTrust(new IntPtr(-1), WinTrust.GenericVerify, ref data);

            return code;
        }
        finally
        {
            Marshal.DestroyStructure<WinCatalog.WinTrustCatalogInfo>(catalogPtr);
            Marshal.FreeHGlobal(catalogPtr);
            Marshal.FreeHGlobal(hashPtr);
        }
    }
}