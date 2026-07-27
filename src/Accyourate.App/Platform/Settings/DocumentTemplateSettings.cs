namespace Accyourate.App.Platform.Settings;

public sealed class DocumentTemplateSettings
{
    public string TemplateName { get; set; } = "Aziendale moderno";
    public string HeaderLayout { get; set; } = "Corporate";
    public string LogoSize { get; set; } = "Medio";
    public string LogoPosition { get; set; } = "Sinistra";
    public string PrimaryColor { get; set; } = "#0A84FF";
    public string SecondaryColor { get; set; } = "#1D1D1F";
    public bool ShowLogo { get; set; } = true;
    public bool ShowCompanyDetails { get; set; } = true;
    public bool ShowDocumentMetadata { get; set; } = true;
    public bool ShowFooter { get; set; } = true;
    public bool ShowSignatures { get; set; } = true;
    public bool ShowQrCodePlaceholder { get; set; }
    public string FooterText { get; set; } = "Documento generato automaticamente da Accyourate Enterprise X";
    public string DocumentVersion { get; set; } = "1.0";
    public string ConfidentialityText { get; set; } = "Documento ad uso interno";
    public bool ShowPageNumber { get; set; } = true;
    public bool ShowPrintTimestamp { get; set; } = true;
    public string LeftSignatureLabel { get; set; } = "Consegnato da";
    public string RightSignatureLabel { get; set; } = "Ricevuto da";
}
