using System.Globalization;
using System.Text;
using QRCoder;

namespace Accyourate.App.Platform.Pdf;

public sealed class SimplePdfWriter
{
    private static readonly Encoding PdfEncoding = Encoding.Latin1;

    public void Write(SimplePdfDocument document, string outputPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? ".");

        var logo = TryLoadJpeg(document.Branding.ShowLogo ? document.Branding.LogoPath : string.Empty);
        var pages = BuildPageStreams(document, logo);
        var pageCount = pages.Count;
        var fontRegularObject = 3 + pageCount * 2;
        var fontBoldObject = fontRegularObject + 1;
        var logoObject = logo is null ? 0 : fontBoldObject + 1;
        var infoObject = logo is null ? fontBoldObject + 1 : logoObject + 1;

        var objects = new List<string>
        {
            "<< /Type /Catalog /Pages 2 0 R >>",
            $"<< /Type /Pages /Kids [{string.Join(" ", Enumerable.Range(0, pageCount).Select(i => $"{3 + i * 2} 0 R"))}] /Count {pageCount} >>"
        };

        for (var i = 0; i < pageCount; i++)
        {
            var pageObj = 3 + i * 2;
            var contentObj = pageObj + 1;
            var xObject = logo is null ? string.Empty : $" /XObject << /Im1 {logoObject} 0 R >>";
            objects.Add($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 {fontRegularObject} 0 R /F2 {fontBoldObject} 0 R >>{xObject} >> /Contents {contentObj} 0 R >>");
            objects.Add($"<< /Length {PdfEncoding.GetByteCount(pages[i])} >>\nstream\n{pages[i]}\nendstream");
        }

        objects.Add("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>");
        objects.Add("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold /Encoding /WinAnsiEncoding >>");

        if (logo is not null)
        {
            var hex = Convert.ToHexString(logo.Bytes) + ">";
            objects.Add($"<< /Type /XObject /Subtype /Image /Width {logo.Width} /Height {logo.Height} /ColorSpace /DeviceRGB /BitsPerComponent 8 /Filter [/ASCIIHexDecode /DCTDecode] /Length {hex.Length} >>\nstream\n{hex}\nendstream");
        }

        objects.Add($"<< /Title ({Esc(document.Title)}) /Author ({Esc(document.Author)}) /Producer (Accyourate Enterprise X PDF Engine) >>");

        var builder = new StringBuilder();
        var offsets = new List<int>();
        builder.AppendLine("%PDF-1.4");
        for (var i = 0; i < objects.Count; i++)
        {
            offsets.Add(PdfEncoding.GetByteCount(builder.ToString()));
            builder.AppendLine($"{i + 1} 0 obj");
            builder.AppendLine(objects[i]);
            builder.AppendLine("endobj");
        }

        var xref = PdfEncoding.GetByteCount(builder.ToString());
        builder.AppendLine("xref");
        builder.AppendLine($"0 {objects.Count + 1}");
        builder.AppendLine("0000000000 65535 f ");
        foreach (var offset in offsets)
            builder.AppendLine($"{offset:D10} 00000 n ");
        builder.AppendLine("trailer");
        builder.AppendLine($"<< /Size {objects.Count + 1} /Root 1 0 R /Info {infoObject} 0 R >>");
        builder.AppendLine("startxref");
        builder.AppendLine(xref.ToString(CultureInfo.InvariantCulture));
        builder.AppendLine("%%EOF");

        File.WriteAllText(outputPath, builder.ToString(), PdfEncoding);
    }

    private static List<string> BuildPageStreams(SimplePdfDocument document, JpegData? logo)
    {
        var pages = new List<string>();
        var content = new StringBuilder();
        var pageNumber = 1;
        var y = StartPage(content, document, logo);

        foreach (var line in document.Lines)
        {
            var requiredHeight = RequiredHeight(line);

            if (y - requiredHeight < 78)
            {
                EndPage(content, document, pageNumber++);
                pages.Add(content.ToString());
                content.Clear();
                y = StartPage(content, document, logo);
            }

            switch (line.Kind)
            {
                case PdfLineKind.SignaturePair:
                    DrawSignaturePair(content, line, y);
                    y -= Math.Max(line.GapAfter, 58);
                    break;
                case PdfLineKind.Section:
                    DrawSection(content, document, line.Text, y);
                    y -= 30;
                    break;
                case PdfLineKind.KeyValue:
                    y -= DrawKeyValue(content, line.Text, line.RightText, line.FontSize, y, line.GapAfter);
                    break;
                case PdfLineKind.Status:
                    y -= DrawStatus(content, document, line.Text, line.RightText, line.FontSize, y, line.GapAfter);
                    break;
                case PdfLineKind.QrPlaceholder:
                    DrawQrPlaceholder(content, line.Text, y);
                    y -= Math.Max(line.GapAfter, 82);
                    break;
                case PdfLineKind.QrCode:
                    DrawQrCode(content, line.RightText, line.Text, y);
                    y -= Math.Max(line.GapAfter, 96);
                    break;
                default:
                    y -= DrawWrappedText(content, line, y);
                    break;
            }
        }

        EndPage(content, document, pageNumber);
        pages.Add(content.ToString());
        return pages;
    }



    private static void DrawSection(StringBuilder builder, SimplePdfDocument document, string text, float y)
    {
        var (r, g, b) = ParseColor(document.Branding.PrimaryColor);
        if (0.2126 * r + 0.7152 * g + 0.0722 * b > 0.82)
            (r, g, b) = (0.04f, 0.52f, 1f);
        builder.AppendLine($"{N(r)} {N(g)} {N(b)} rg");
        builder.AppendLine($"54 {N(y - 7)} 487 22 re f");
        builder.AppendLine("1 1 1 rg");
        builder.AppendLine($"BT /F2 10.5 Tf 64 {Y(y)} Td ({Esc(text.ToUpperInvariant())}) Tj ET");
        builder.AppendLine("0 0 0 rg");
    }

    private static float DrawKeyValue(
        StringBuilder builder,
        string label,
        string value,
        int fontSize,
        float y,
        float gapAfter)
    {
        var wrapped = WrapText(value, fontSize, 310);
        var lineHeight = Math.Max(fontSize + 3, 13);
        var height = Math.Max(28, wrapped.Count * lineHeight + gapAfter + 4);
        var lastBaseline = y - (wrapped.Count - 1) * lineHeight;
        var nextBaseline = y - height;
        var separatorY = (lastBaseline + nextBaseline) / 2f;
        builder.AppendLine("0.93 0.93 0.93 RG");
        builder.AppendLine($"54 {N(separatorY)} m 541 {N(separatorY)} l S");
        builder.AppendLine($"BT /F2 9.5 Tf 62 {Y(y)} Td ({Esc(label)}) Tj ET");
        for (var index = 0; index < wrapped.Count; index++)
            builder.AppendLine($"BT /F1 {fontSize} Tf 225 {Y(y - index * lineHeight)} Td ({Esc(wrapped[index])}) Tj ET");
        return height;
    }

    private static float DrawStatus(
        StringBuilder builder,
        SimplePdfDocument document,
        string label,
        string value,
        int fontSize,
        float y,
        float gapAfter)
    {
        var wrapped = WrapText(value, fontSize, 310);
        var lineHeight = Math.Max(fontSize + 3, 13);
        var height = Math.Max(28, wrapped.Count * lineHeight + gapAfter + 4);
        builder.AppendLine("0.96 0.96 0.96 rg");
        builder.AppendLine($"54 {N(y - height + 7)} 487 {N(height)} re f");
        var (r, g, b) = ParseColor(document.Branding.PrimaryColor);
        builder.AppendLine($"{N(r)} {N(g)} {N(b)} rg");
        builder.AppendLine($"54 {N(y - height + 7)} 4 {N(height)} re f");
        builder.AppendLine("0 0 0 rg");
        builder.AppendLine($"BT /F2 9.5 Tf 64 {Y(y)} Td ({Esc(label)}) Tj ET");
        for (var index = 0; index < wrapped.Count; index++)
            builder.AppendLine($"BT /F2 {fontSize} Tf 225 {Y(y - index * lineHeight)} Td ({Esc(wrapped[index])}) Tj ET");
        builder.AppendLine("0 0 0 rg");
        return height;
    }

    private static float DrawWrappedText(StringBuilder builder, PdfTextLine line, float y)
    {
        if (string.IsNullOrWhiteSpace(line.Text))
            return Math.Max(line.GapAfter, 10);

        var wrapped = WrapText(line.Text, line.FontSize, 487);
        var lineHeight = Math.Max(line.FontSize + 3, 13);
        for (var index = 0; index < wrapped.Count; index++)
        {
            builder.AppendLine(
                $"BT /{(line.Bold ? "F2" : "F1")} {line.FontSize} Tf 54 {Y(y - index * lineHeight)} Td ({Esc(wrapped[index])}) Tj ET");
        }

        return wrapped.Count * lineHeight + line.GapAfter;
    }

    private static float RequiredHeight(PdfTextLine line)
    {
        return line.Kind switch
        {
            PdfLineKind.SignaturePair => 64f,
            PdfLineKind.QrPlaceholder => 90f,
            PdfLineKind.QrCode => 104f,
            PdfLineKind.Section => 30f,
            PdfLineKind.KeyValue => WrappedHeight(line.RightText, line.FontSize, 310, line.GapAfter + 4, 28),
            PdfLineKind.Status => WrappedHeight(line.RightText, line.FontSize, 310, line.GapAfter + 4, 28),
            _ => WrappedHeight(line.Text, line.FontSize, 487, line.GapAfter, 12)
        };
    }

    private static float WrappedHeight(
        string text,
        int fontSize,
        double width,
        float gapAfter,
        float minimum)
    {
        var count = Math.Max(1, WrapText(text, fontSize, width).Count);
        var lineHeight = Math.Max(fontSize + 3, 13);
        return Math.Max(minimum, count * lineHeight + gapAfter);
    }

    private static IReadOnlyList<string> WrapText(string text, double fontSize, double maxWidth)
    {
        var normalized = (text ?? string.Empty)
            .Replace("\r\n", "\n")
            .Replace('\r', '\n');
        var result = new List<string>();

        foreach (var paragraph in normalized.Split('\n'))
        {
            var words = paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0)
            {
                result.Add(string.Empty);
                continue;
            }

            var current = new StringBuilder();
            foreach (var word in words)
            {
                var candidate = current.Length == 0 ? word : $"{current} {word}";
                if (ApproximateWidth(candidate, fontSize) <= maxWidth)
                {
                    current.Clear();
                    current.Append(candidate);
                    continue;
                }

                if (current.Length > 0)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }

                if (ApproximateWidth(word, fontSize) <= maxWidth)
                {
                    current.Append(word);
                    continue;
                }

                var chunk = new StringBuilder();
                foreach (var character in word)
                {
                    if (chunk.Length > 0 && ApproximateWidth($"{chunk}{character}", fontSize) > maxWidth)
                    {
                        result.Add(chunk.ToString());
                        chunk.Clear();
                    }
                    chunk.Append(character);
                }
                current.Append(chunk);
            }

            if (current.Length > 0)
                result.Add(current.ToString());
        }

        return result.Count == 0 ? new[] { string.Empty } : result;
    }

    private static double ApproximateWidth(string text, double fontSize)
    {
        var units = 0d;
        foreach (var character in text)
        {
            units += character switch
            {
                'i' or 'l' or 'I' or '.' or ',' or ':' or ';' or '!' or '|' => 0.28,
                'm' or 'w' or 'M' or 'W' or '@' => 0.82,
                ' ' => 0.3,
                _ => 0.54
            };
        }
        return units * fontSize;
    }

    private static void DrawQrPlaceholder(StringBuilder builder, string caption, float y)
    {
        const double size = 68;
        const double x = 473;
        var bottom = y - 64;
        builder.AppendLine("0.75 0.75 0.75 RG");
        builder.AppendLine($"{N(x)} {N(bottom)} {N(size)} {N(size)} re S");
        builder.AppendLine($"{N(x)} {N(bottom)} m {N(x + size)} {N(bottom + size)} l S");
        builder.AppendLine($"{N(x + size)} {N(bottom)} m {N(x)} {N(bottom + size)} l S");
        builder.AppendLine($"BT /F2 7 Tf {N(x + 9)} {N(bottom - 11)} Td ({Esc(caption)}) Tj ET");
    }

    private static void DrawQrCode(StringBuilder builder, string payload, string caption, float y)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            DrawQrPlaceholder(builder, caption, y);
            return;
        }

        using var data = QRCodeGenerator.GenerateQrCode(
            payload,
            QRCodeGenerator.ECCLevel.M,
            forceUtf8: true,
            utf8BOM: false,
            eciMode: QRCodeGenerator.EciMode.Utf8);

        const double x = 447d;
        const double size = 94d;
        const int quietZone = 4;
        var modules = data.ModuleMatrix;
        var totalModules = modules.Count + quietZone * 2;
        var moduleSize = size / totalModules;
        var bottom = y - size + 4;

        builder.AppendLine("1 1 1 rg");
        builder.AppendLine($"{N(x)} {N(bottom)} {N(size)} {N(size)} re f");
        builder.AppendLine("0 0 0 rg");

        for (var row = 0; row < modules.Count; row++)
        {
            for (var column = 0; column < modules[row].Count; column++)
            {
                if (!modules[row][column])
                    continue;

                var moduleX = x + (column + quietZone) * moduleSize;
                var moduleY = bottom + size - (row + quietZone + 1) * moduleSize;
                builder.AppendLine($"{N(moduleX)} {N(moduleY)} {N(moduleSize + 0.02)} {N(moduleSize + 0.02)} re f");
            }
        }

        builder.AppendLine("0 0 0 rg");
        builder.AppendLine($"BT /F2 7 Tf {N(x)} {N(bottom - 12)} Td ({Esc(caption)}) Tj ET");
    }
    private static void DrawSignaturePair(StringBuilder builder, PdfTextLine line, float y)
    {
        const double leftX = 54d;
        const double rightX = 318d;
        const double lineWidth = 223d;
        var labelY = y;
        var signatureY = y - 31d;

        builder.AppendLine($"BT /F1 {line.FontSize} Tf {N(leftX)} {Y(labelY)} Td ({Esc(line.Text)}) Tj ET");
        builder.AppendLine($"BT /F1 {line.FontSize} Tf {N(rightX)} {Y(labelY)} Td ({Esc(line.RightText)}) Tj ET");
        builder.AppendLine("0.45 0.45 0.45 RG");
        builder.AppendLine($"{N(leftX)} {N(signatureY)} m {N(leftX + lineWidth)} {N(signatureY)} l S");
        builder.AppendLine($"{N(rightX)} {N(signatureY)} m {N(rightX + lineWidth)} {N(signatureY)} l S");
    }

    private static float StartPage(StringBuilder builder, SimplePdfDocument document, JpegData? logo)
    {
        var layout = NormalizeHeaderLayout(document.Branding.HeaderLayout);
        var headerHeight = layout switch
        {
            "Enterprise" => 142d,
            "Compatta" => 78d,
            _ => 112d
        };
        var headerBottom = 842d - headerHeight;
        var (r, g, b) = ParseColor(document.Branding.PrimaryColor);

        builder.AppendLine("q");
        builder.AppendLine($"{N(r)} {N(g)} {N(b)} rg");
        builder.AppendLine($"0 {N(headerBottom)} 595 {N(headerHeight)} re f");
        builder.AppendLine("Q");

        var details = GetCompanyDetailLines(document, layout);
        var company = string.IsNullOrWhiteSpace(document.Branding.CompanyName) ? document.Author : document.Branding.CompanyName;

        if (layout == "Compatta")
            DrawCompactHeader(builder, document, logo, company, details, headerBottom);
        else if (layout == "Enterprise")
            DrawEnterpriseHeader(builder, document, logo, company, details, headerBottom);
        else
            DrawCorporateHeader(builder, document, logo, company, details, headerBottom);

        builder.AppendLine("0.88 0.88 0.88 RG");
        builder.AppendLine($"54 {N(headerBottom - 8)} m 541 {N(headerBottom - 8)} l S");
        return (float)(headerBottom - 34);
    }

    private static void DrawCorporateHeader(StringBuilder builder, SimplePdfDocument document, JpegData? logo, string company, IReadOnlyList<string> details, double headerBottom)
    {
        const double top = 824d;
        var companyX = 190d;
        if (logo is not null)
            DrawLogo(builder, logo, 54, headerBottom + 22, 118, 66);

        var companyLines = WrapText(company, 15, 195).Take(2).ToList();
        for (var index = 0; index < companyLines.Count; index++)
            builder.AppendLine($"BT /F2 15 Tf {N(companyX)} {N(top - index * 17)} Td ({Esc(companyLines[index])}) Tj ET");
        DrawDetailLines(builder, details, companyX, top - Math.Max(18, companyLines.Count * 17 + 2), 8.2, 12, 3);
        DrawDocumentBlock(builder, document, 407, top, 8.2);
    }

    private static void DrawEnterpriseHeader(StringBuilder builder, SimplePdfDocument document, JpegData? logo, string company, IReadOnlyList<string> details, double headerBottom)
    {
        const double top = 824d;
        if (logo is not null)
            DrawLogo(builder, logo, 54, headerBottom + 72, 150, 50);

        builder.AppendLine($"BT /F2 16 Tf 54 {N(top - 58)} Td ({Esc(company)}) Tj ET");
        DrawDetailLines(builder, details, 54, top - 77, 8.2, 12, 5);
        DrawDocumentBlock(builder, document, 394, top - 58, 8.2);
    }

    private static void DrawCompactHeader(StringBuilder builder, SimplePdfDocument document, JpegData? logo, string company, IReadOnlyList<string> details, double headerBottom)
    {
        const double top = 820d;
        var companyX = 126d;
        if (logo is not null)
            DrawLogo(builder, logo, 54, headerBottom + 14, 58, 48);

        builder.AppendLine($"BT /F2 14 Tf {N(companyX)} {N(top)} Td ({Esc(company)}) Tj ET");
        DrawDetailLines(builder, details, companyX, top - 17, 7.6, 10, 1);
        DrawDocumentBlock(builder, document, 407, top, 7.6);
    }

    private static void DrawLogo(StringBuilder builder, JpegData logo, double x, double y, double maxWidth, double maxHeight)
    {
        var scale = Math.Min(maxWidth / Math.Max(logo.Width, 1), maxHeight / Math.Max(logo.Height, 1));
        var width = Math.Max(1, logo.Width * scale);
        var height = Math.Max(1, logo.Height * scale);
        var drawX = x + (maxWidth - width) / 2d;
        var drawY = y + (maxHeight - height) / 2d;
        builder.AppendLine("q");
        builder.AppendLine($"{N(width)} 0 0 {N(height)} {N(drawX)} {N(drawY)} cm");
        builder.AppendLine("/Im1 Do");
        builder.AppendLine("Q");
    }

    private static void DrawDetailLines(StringBuilder builder, IReadOnlyList<string> details, double x, double startY, double fontSize, double lineHeight, int maxLines)
    {
        for (var i = 0; i < Math.Min(details.Count, maxLines); i++)
            builder.AppendLine($"BT /F1 {N(fontSize)} Tf {N(x)} {N(startY - i * lineHeight)} Td ({Esc(details[i])}) Tj ET");
    }

    private static void DrawDocumentBlock(StringBuilder builder, SimplePdfDocument document, double x, double y, double fontSize)
    {
        if (!string.IsNullOrWhiteSpace(document.Branding.DocumentLabel))
            builder.AppendLine($"BT /F2 10.5 Tf {N(x)} {N(y)} Td ({Esc(document.Branding.DocumentLabel)}) Tj ET");

        if (!document.Branding.ShowDocumentMetadata)
            return;

        var code = string.IsNullOrWhiteSpace(document.Branding.DocumentCode) ? "" : document.Branding.DocumentCode;
        if (!string.IsNullOrWhiteSpace(code))
            builder.AppendLine($"BT /F1 {N(fontSize)} Tf {N(x)} {N(y - 17)} Td ({Esc(code)}) Tj ET");
        builder.AppendLine($"BT /F1 {N(fontSize)} Tf {N(x)} {N(y - 30)} Td ({Esc($"Data: {DateTime.Now:dd/MM/yyyy}")}) Tj ET");
    }

    private static IReadOnlyList<string> GetCompanyDetailLines(SimplePdfDocument document, string layout)
    {
        if (!document.Branding.ShowCompanyDetails)
            return Array.Empty<string>();

        var lines = document.Branding.CompanyDetailLines
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .ToList();
        if (lines.Count == 0 && !string.IsNullOrWhiteSpace(document.Branding.CompanyDetails))
            lines.Add(document.Branding.CompanyDetails.Trim());

        return layout == "Compatta" ? lines.Take(1).ToList() : lines;
    }

    private static string NormalizeHeaderLayout(string? value)
    {
        if (string.Equals(value, "Enterprise", StringComparison.OrdinalIgnoreCase)) return "Enterprise";
        if (string.Equals(value, "Compatta", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "Intestazione compatta", StringComparison.OrdinalIgnoreCase)) return "Compatta";
        return "Corporate";
    }

    private static void EndPage(StringBuilder builder, SimplePdfDocument document, int pageNumber)
    {
        if (!document.Branding.ShowFooter)
            return;

        builder.AppendLine("0.88 0.88 0.88 RG");
        builder.AppendLine("54 58 m 541 58 l S");
        var footer = string.IsNullOrWhiteSpace(document.Branding.FooterText)
            ? "Documento generato automaticamente da Accyourate Enterprise X"
            : document.Branding.FooterText;
        builder.AppendLine($"BT /F1 7.5 Tf 54 42 Td ({Esc(footer)}) Tj ET");
        var metadata = new List<string>();
        if (!string.IsNullOrWhiteSpace(document.Branding.DocumentVersion)) metadata.Add($"Rev. {document.Branding.DocumentVersion}");
        if (document.Branding.ShowPrintTimestamp) metadata.Add($"Stampato il {DateTime.Now:dd/MM/yyyy HH:mm}");
        if (!string.IsNullOrWhiteSpace(document.Branding.ConfidentialityText)) metadata.Add(document.Branding.ConfidentialityText);
        if (metadata.Count > 0)
            builder.AppendLine($"BT /F1 6.8 Tf 54 29 Td ({Esc(string.Join(" | ", metadata))}) Tj ET");
        if (document.Branding.ShowPageNumber)
            builder.AppendLine($"BT /F2 8 Tf 500 40 Td ({Esc($"Pagina {pageNumber}")}) Tj ET");
    }

    private static (double R, double G, double B) ParseColor(string hex)
    {
        try
        {
            var value = (hex ?? string.Empty).Trim().TrimStart('#');
            if (value.Length != 6)
                return (0.039, 0.518, 1);
            return (
                Convert.ToInt32(value[..2], 16) / 255d,
                Convert.ToInt32(value.Substring(2, 2), 16) / 255d,
                Convert.ToInt32(value.Substring(4, 2), 16) / 255d);
        }
        catch
        {
            return (0.039, 0.518, 1);
        }
    }

    private static JpegData? TryLoadJpeg(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            return null;

        var extension = Path.GetExtension(path);
        if (!extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) &&
            !extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase))
            return null;

        try
        {
            var bytes = File.ReadAllBytes(path);
            var dimensions = ReadJpegDimensions(bytes);
            return dimensions is null ? null : new JpegData(bytes, dimensions.Value.Width, dimensions.Value.Height);
        }
        catch
        {
            return null;
        }
    }

    private static (int Width, int Height)? ReadJpegDimensions(byte[] data)
    {
        if (data.Length < 4 || data[0] != 0xFF || data[1] != 0xD8)
            return null;

        var index = 2;
        while (index + 8 < data.Length)
        {
            if (data[index] != 0xFF)
            {
                index++;
                continue;
            }

            var marker = data[index + 1];
            index += 2;
            if (marker is 0xD8 or 0xD9)
                continue;
            if (index + 2 > data.Length)
                break;

            var length = (data[index] << 8) + data[index + 1];
            if (length < 2 || index + length > data.Length)
                break;

            if (marker is >= 0xC0 and <= 0xC3)
            {
                var height = (data[index + 3] << 8) + data[index + 4];
                var width = (data[index + 5] << 8) + data[index + 6];
                return (width, height);
            }

            index += length;
        }

        return null;
    }

    private static string Esc(string text) => (text ?? string.Empty)
        .Replace("\\", "\\\\")
        .Replace("(", "\\(")
        .Replace(")", "\\)")
        .Replace("\r", " ")
        .Replace("\n", " ");

    private static string Y(float value) => value.ToString("0.##", CultureInfo.InvariantCulture);
    private static string N(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);

    private sealed record JpegData(byte[] Bytes, int Width, int Height);
}
