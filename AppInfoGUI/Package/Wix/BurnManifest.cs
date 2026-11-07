using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace AppInfoGUI.Package.Wix;

// Reads the manifest of a bundle (an XML file). Nothing in it is carried out.
public static class BurnManifest
{
    public static BurnBundle Parse(byte[] xml)
    {
        var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null };
        using var stream = new MemoryStream(xml);
        using var reader = XmlReader.Create(stream, settings);
        XElement root = XDocument.Load(reader).Root ?? throw new InvalidDataException("the bundle's manifest is empty");

        // Every file of the bundle, so the packages and the interface can point to them
        Dictionary<string, BurnPayload> payloads = root.Descendants()
            .Where(e => e.Name.LocalName == "Payload" && e.Attribute("Id") != null)
            .Select(ToPayload)
            .GroupBy(p => p.Id)
            .ToDictionary(g => g.Key, g => g.First());

        XElement? registration = root.Descendants().FirstOrDefault(e => e.Name.LocalName == "Arp");

        return new BurnBundle(
            Text(registration, "DisplayName"),
            Text(registration, "DisplayVersion"),
            Text(registration, "Publisher"),
            Packages(root, payloads),
            InterfaceFiles(root, payloads));
    }

    static List<BurnPackage> Packages(XElement root, Dictionary<string, BurnPayload> payloads)
    {
        XElement? chain = root.Elements().FirstOrDefault(e => e.Name.LocalName == "Chain");
        if (chain == null) return new();

        return chain.Elements()
            .Where(e => e.Name.LocalName.EndsWith("Package"))
            .Select(package =>
            {
                string? reference = package.Elements().FirstOrDefault(e => e.Name.LocalName == "PayloadRef")?.Attribute("Id")?.Value;
                payloads.TryGetValue(reference ?? "", out BurnPayload? payload);

                string kind = package.Name.LocalName.Replace("Package", "");
                return new BurnPackage(kind, Text(package, "Id") ?? "", Number(package, "Size"), payload);
            })
            .ToList();
    }

    static List<BurnPayload> InterfaceFiles(XElement root, Dictionary<string, BurnPayload> payloads) =>
        root.Elements().Where(e => e.Name.LocalName == "UX")
            .SelectMany(ux => ux.Elements().Where(e => e.Name.LocalName == "Payload"))
            .Select(e => payloads.GetValueOrDefault(Text(e, "Id") ?? ""))
            .OfType<BurnPayload>()
            .ToList();

    static BurnPayload ToPayload(XElement e) => new(
        Text(e, "Id") ?? "",
        Text(e, "FilePath") ?? Text(e, "Id") ?? "",
        Number(e, "FileSize"),
        Text(e, "Packaging") ?? "",
        Text(e, "DownloadUrl"),
        Text(e, "SourcePath"));

    static string? Text(XElement? e, string attribute) => e?.Attribute(attribute)?.Value;

    static long Number(XElement e, string attribute) =>
        long.TryParse(Text(e, attribute), out long value) ? value : -1;
}
