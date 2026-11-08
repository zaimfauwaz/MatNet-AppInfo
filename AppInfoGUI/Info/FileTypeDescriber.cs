using System.IO;

namespace AppInfoGUI.Info;

internal static class FileTypeDescriber
{
    // Names the file type from its extension
    public static string Describe(string path)
    {
        string extension = Path.GetExtension(path).ToLowerInvariant();
        return $"{NameFor(extension)} ({extension})";
    }

    // Names what the first bytes say, and warns if that does not fit the name
    public static string DescribeContents(string path)
    {
        ContentInfo content = FileContent.Detect(path);
        return Mismatch(path, content) == null ? content.Name : content.Name + " - does not match the name";
    }

    // Null when the contents fit the name, or when the name is not one we can check
    public static string? Mismatch(string path, ContentInfo content)
    {
        string extension = Path.GetExtension(path).ToLowerInvariant();
        ContentType? expected = Expected(extension);

        // Documents have many inner formats, so for them only a hidden program is worth a line
        if (expected == null && content.Type == ContentType.Program && IsDocumentName(extension))
            return $"The name says {NameFor(extension)} ({extension}), but the contents are {content.Name}";

        if (expected == null || content.Type == expected)
            return null;

        return $"The name says {NameFor(extension)} ({extension}), but the contents are {content.Name}";
    }

    static bool IsDocumentName(string extension) => extension is ".txt" or ".doc" or ".docx" or ".xls" or ".xlsx";

    static string NameFor(string extension) => extension switch
    {
        ".exe" => "Application",
        ".scr" => "Screen saver",
        ".dll" => "Dynamic-link library",
        ".rll" => "Resource library",
        ".ocx" => "ActiveX control",
        ".sys" => "Driver",
        ".msi" => "Windows Installer package",
        ".msp" => "Windows Installer patch",
        ".pdf" => "PDF document",
        ".png" or ".jpg" or ".jpeg" or ".gif" => "Picture",
        ".zip" => "ZIP archive",
        ".7z" => "7-Zip archive",
        ".rar" => "RAR archive",
        ".cab" => "Cabinet (CAB) archive",
        ".txt" => "Text file",
        ".doc" or ".docx" => "Word document",
        ".xls" or ".xlsx" => "Excel workbook",
        _ => "Other file"
    };

    static ContentType? Expected(string extension) => extension switch
    {
        ".exe" or ".scr" or ".dll" or ".rll" or ".ocx" or ".sys" => ContentType.Program,
        ".msi" or ".msp" => ContentType.OleFile,
        ".pdf" => ContentType.Pdf,
        ".png" or ".jpg" or ".jpeg" or ".gif" => ContentType.Picture,
        ".zip" => ContentType.Zip,
        ".7z" => ContentType.SevenZip,
        ".rar" => ContentType.Rar,
        ".cab" => ContentType.Cabinet,
        _ => null
    };
}
