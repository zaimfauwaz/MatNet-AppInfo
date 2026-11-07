using System.Text.Json;

namespace AppInfoGUI.Driver;

// Reads the LOLDrivers "drivers.json" list. It walks the whole file looking for SHA256 values,
// so a small change in the list's layout does not break it.
internal static class VulnerableDriverParser
{
    public static void Fill(VulnerableDriverList list, string json)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        Walk(list, document.RootElement, "Known vulnerable driver", false);
    }

    // name: the closest driver name seen so far. signed: true inside an "Authentihash" block.
    static void Walk(VulnerableDriverList list, JsonElement element, string name, bool signed)
    {
        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement item in element.EnumerateArray())
                Walk(list, item, name, signed);
        }
        else if (element.ValueKind == JsonValueKind.Object)
        {
            name = NameOf(element) ?? name;

            foreach (JsonProperty property in element.EnumerateObject())
            {
                if (property.NameEquals("SHA256") && IsSha256(property.Value))
                    Add(list, property.Value.GetString()!, name, signed);
                else
                    Walk(list, property.Value, name, signed || property.NameEquals("Authentihash"));
            }
        }
    }

    // A driver entry names itself with a file name, or failing that its first tag
    static string? NameOf(JsonElement entry)
    {
        if (entry.TryGetProperty("Filename", out JsonElement file) && file.ValueKind == JsonValueKind.String)
            return file.GetString();

        if (entry.TryGetProperty("Tags", out JsonElement tags) && tags.ValueKind == JsonValueKind.Array
            && tags.GetArrayLength() > 0 && tags[0].ValueKind == JsonValueKind.String)
            return tags[0].GetString();

        return null;
    }

    static bool IsSha256(JsonElement value) =>
        value.ValueKind == JsonValueKind.String && value.GetString() is { Length: 64 } text && text.All(Uri.IsHexDigit);

    static void Add(VulnerableDriverList list, string hash, string name, bool signed)
    {
        if (signed) list.AddSignedHash(hash, name);
        else list.AddFileHash(hash, name);
    }
}
