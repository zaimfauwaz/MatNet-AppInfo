using AppInfoGUI.Info;
using System.IO;
using System.Reflection.PortableExecutable;

namespace AppInfoGUI.PE;

internal static class PeSectionRows
{
    const double HighEntropy = 7.2;   // close to random data: packed or encrypted

    static readonly string[] PackerNames = { "UPX", ".aspack", ".adata", ".themida", ".vmp", ".petite", ".mpress" };

    public static void Fill(List<DetailRow> rows, Stream stream, PEHeaders headers)
    {
        foreach (SectionHeader section in headers.SectionHeaders)
        {
            string name = section.Name;
            rows.Add(new(name, Safe.Text(() => Describe(stream, section))));
        }
    }

    static string Describe(Stream stream, SectionHeader section)
    {
        string size = TextFormat.Size(section.SizeOfRawData);
        if (section.SizeOfRawData == 0)
            return size + ", empty on disk";

        double entropy = Entropy(stream, section.PointerToRawData, section.SizeOfRawData);
        string text = $"{size}, entropy {entropy:0.00} of 8";

        if (entropy >= HighEntropy)
            text += " - high (packed or encrypted?)";

        if (PackerNames.Any(p => section.Name.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            text += " - known packer section name";

        return text;
    }

    // 0 = all the same byte, 8 = completely random
    static double Entropy(Stream stream, int start, int length)
    {
        var counts = new long[256];
        byte[] buffer = new byte[64 * 1024];

        stream.Position = start;
        int left = length;
        while (left > 0)
        {
            int read = stream.Read(buffer, 0, Math.Min(buffer.Length, left));
            if (read == 0) break;

            for (int i = 0; i < read; i++)
                counts[buffer[i]]++;
            left -= read;
        }

        long total = length - left;
        double entropy = 0;
        foreach (long count in counts)
        {
            if (count == 0) continue;
            double p = (double)count / total;
            entropy -= p * Math.Log2(p);
        }

        return entropy;
    }
}
