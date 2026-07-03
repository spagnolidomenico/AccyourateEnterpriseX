using System.Globalization;
using System.Text;

namespace Accyourate.App.Platform.Pdf;

public sealed class SimplePdfWriter
{
    public void Write(SimplePdfDocument document, string outputPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? ".");
        var pages = BuildPageStreams(document.Lines);
        var objects = new List<string>
        {
            "<< /Type /Catalog /Pages 2 0 R >>",
            $"<< /Type /Pages /Kids [{string.Join(" ", Enumerable.Range(0, pages.Count).Select(i => $"{3 + i * 2} 0 R"))}] /Count {pages.Count} >>"
        };

        for (var i = 0; i < pages.Count; i++)
        {
            var pageObj = 3 + i * 2;
            var contentObj = pageObj + 1;
            objects.Add($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 {3 + pages.Count * 2} 0 R /F2 {4 + pages.Count * 2} 0 R >> >> /Contents {contentObj} 0 R >>");
            objects.Add($"<< /Length {Encoding.ASCII.GetByteCount(pages[i])} >>\nstream\n{pages[i]}\nendstream");
        }

        objects.Add("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>");
        objects.Add("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold >>");
        var info = objects.Count + 1;
        objects.Add($"<< /Title ({Esc(document.Title)}) /Author ({Esc(document.Author)}) /Producer (Accyourate Enterprise X PDF Engine) >>");

        var b = new StringBuilder();
        var offsets = new List<int>();
        b.AppendLine("%PDF-1.4");
        for (var i = 0; i < objects.Count; i++)
        {
            offsets.Add(Encoding.ASCII.GetByteCount(b.ToString()));
            b.AppendLine($"{i + 1} 0 obj");
            b.AppendLine(objects[i]);
            b.AppendLine("endobj");
        }
        var xref = Encoding.ASCII.GetByteCount(b.ToString());
        b.AppendLine("xref");
        b.AppendLine($"0 {objects.Count + 1}");
        b.AppendLine("0000000000 65535 f ");
        foreach (var o in offsets) b.AppendLine($"{o:D10} 00000 n ");
        b.AppendLine("trailer");
        b.AppendLine($"<< /Size {objects.Count + 1} /Root 1 0 R /Info {info} 0 R >>");
        b.AppendLine("startxref");
        b.AppendLine(xref.ToString(CultureInfo.InvariantCulture));
        b.AppendLine("%%EOF");
        File.WriteAllText(outputPath, b.ToString(), Encoding.ASCII);
    }

    private static List<string> BuildPageStreams(IReadOnlyList<PdfTextLine> lines)
    {
        var pages = new List<string>();
        var c = new StringBuilder();
        var y = 790f;
        Start(c);
        foreach (var l in lines)
        {
            if (y < 70) { End(c); pages.Add(c.ToString()); c.Clear(); Start(c); y = 790f; }
            if (!string.IsNullOrWhiteSpace(l.Text))
                c.AppendLine($"BT /{(l.Bold ? "F2" : "F1")} {l.FontSize} Tf 54 {Y(y)} Td ({Esc(l.Text)}) Tj ET");
            y -= Math.Max(l.FontSize + l.GapAfter, 12);
        }
        End(c); pages.Add(c.ToString()); return pages;
    }

    private static void Start(StringBuilder b) { b.AppendLine("q"); b.AppendLine("0.95 0.95 0.95 rg"); b.AppendLine("54 812 487 1 re f"); b.AppendLine("Q"); }
    private static void End(StringBuilder b) => b.AppendLine("BT /F1 8 Tf 54 34 Td (Generato da Accyourate Enterprise X) Tj ET");
    private static string Esc(string t) => (t ?? "").Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)").Replace("\r", " ").Replace("\n", " ");
    private static string Y(float v) => v.ToString("0.##", CultureInfo.InvariantCulture);
}
