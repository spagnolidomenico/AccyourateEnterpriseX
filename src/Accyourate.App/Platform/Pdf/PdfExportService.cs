namespace Accyourate.App.Platform.Pdf;

public sealed class PdfExportService
{
    private readonly SimplePdfWriter _writer = new();

    public string Export(SimplePdfDocument document, string folder, string fileName)
    {
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, MakeSafeFileName(fileName));
        if (!path.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            path += ".pdf";
        _writer.Write(document, path);
        return path;
    }

    private static string MakeSafeFileName(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string((value ?? "document").Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray());
        return string.IsNullOrWhiteSpace(cleaned) ? $"document-{DateTime.Now:yyyyMMddHHmmss}.pdf" : cleaned;
    }
}
