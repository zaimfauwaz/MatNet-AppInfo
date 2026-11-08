using System.IO;
using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;

namespace AppInfoGUI.Strings;

// Reads the file and lists the interesting text in it. It only reads. Nothing is run or unpacked.
public static class StringScanner
{
    const long ReadLimit = 64L * 1024 * 1024;   // bigger files are read only this far
    const int MinLength = 6;
    const int PerKind = 200;
    const int ShownLength = 300;

    public static StringsResult Read(string path)
    {
        var result = new StringsResult();
        var seen = new HashSet<(string Kind, string Text)>();
        var counts = StringKinds.All.ToDictionary(k => k, _ => 0);
        bool capped = false;

        void Collect(string text, long at, int bytesPerChar)
        {
            try
            {
                foreach ((string kind, string found, int index) in StringClassifier.Find(text))
                {
                    if (counts[kind] >= PerKind) { capped = true; continue; }
                    if (!seen.Add((kind, found))) continue;

                    counts[kind]++;
                    string shown = found.Length > ShownLength ? found[..ShownLength] + "…" : found;
                    result.Items.Add(new FoundString(kind, shown, at + (long)index * bytesPerChar));
                }
            }
            catch (RegexMatchTimeoutException)
            {
                // an odd string that is slow to match is skipped
            }
        }

        var plain = new RunFinder(false, MinLength, (t, at) => Collect(t, at, 1));
        var wide = new RunFinder(true, MinLength, (t, at) => Collect(t, at, 2));

        (long skipStart, long skipEnd) = SignatureBlock(path);

        using FileStream stream = File.OpenRead(path);
        var buffer = new byte[1024 * 1024];
        long total = 0;
        int read;

        while (total < ReadLimit && (read = stream.Read(buffer, 0, buffer.Length)) > 0)
        {
            BlankOut(buffer, read, total, skipStart, skipEnd);
            plain.Feed(buffer.AsSpan(0, read), total);
            wide.Feed(buffer.AsSpan(0, read), total);
            total += read;
        }

        plain.End();
        wide.End();

        result.Items.Sort((a, b) =>
        {
            int byKind = Array.IndexOf(StringKinds.All, a.Kind).CompareTo(Array.IndexOf(StringKinds.All, b.Kind));
            return byKind != 0 ? byKind : a.Offset.CompareTo(b.Offset);
        });

        result.Message = Describe(total >= ReadLimit && stream.Position < stream.Length, capped, result.Items.Count);
        return result;
    }

    // The signature block holds certificate web addresses packed between binary bytes. The Digital Signatures tab covers it.
    static (long Start, long End) SignatureBlock(string path)
    {
        try
        {
            using FileStream stream = File.OpenRead(path);
            using var pe = new PEReader(stream);

            DirectoryEntry table = pe.PEHeaders.PEHeader!.CertificateTableDirectory;
            return (table.RelativeVirtualAddress, (long)table.RelativeVirtualAddress + table.Size);     // for this entry it is a file offset
        }
        catch (Exception)
        {
            return (0, 0);      // not a program file, so nothing is skipped
        }
    }

    // Zero bytes end every run of text, so the skipped part can simply be zeroed
    static void BlankOut(byte[] buffer, int length, long bufferStart, long skipStart, long skipEnd)
    {
        long from = Math.Max(skipStart, bufferStart);
        long to = Math.Min(skipEnd, bufferStart + length);

        if (to > from)
            Array.Clear(buffer, (int)(from - bufferStart), (int)(to - from));
    }

    static string Describe(bool partial, bool capped, int found)
    {
        var parts = new List<string>();

        if (found == 0)
            parts.Add("No web addresses, IP addresses, registry keys, file paths or emails were found.");
        if (partial)
            parts.Add("Only the first 64 MB of the file were read.");
        if (capped)
            parts.Add($"Showing the first {PerKind} of each kind.");

        return string.Join(" ", parts);
    }
}
