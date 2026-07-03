namespace Accyourate.App.Platform.Pdf;

public sealed class PdfTextLine
{
    public string Text { get; set; } = string.Empty;
    public int FontSize { get; set; } = 11;
    public bool Bold { get; set; }
    public float GapAfter { get; set; } = 8;
}

public sealed class SimplePdfDocument
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = "Accyourate Enterprise X";
    public List<PdfTextLine> Lines { get; } = new();

    public void AddTitle(string text) => Lines.Add(new PdfTextLine { Text = text, FontSize = 20, Bold = true, GapAfter = 16 });
    public void AddHeading(string text) => Lines.Add(new PdfTextLine { Text = text, FontSize = 14, Bold = true, GapAfter = 10 });
    public void AddText(string text, int fontSize = 11) => Lines.Add(new PdfTextLine { Text = text, FontSize = fontSize, GapAfter = 7 });
    public void AddBlank(float gap = 10) => Lines.Add(new PdfTextLine { Text = string.Empty, FontSize = 11, GapAfter = gap });
    public void AddSignature(string label)
    {
        AddBlank(16);
        AddText(label);
        AddText("________________________________________", 12);
    }
}
