namespace Accyourate.App.Platform.Settings;

public sealed class ApplicationSettings
{
    public CompanySettings Company { get; set; } = new();
    public NumberingSettings Numbering { get; set; } = new();
    public DocumentSettings Documents { get; set; } = new();
    public DocumentTemplateSettings DocumentTemplate { get; set; } = new();
    public string ThemeMode { get; set; } = "Light";
    public string Language { get; set; } = "it-IT";
    public string VersionChannel { get; set; } = "Beta";
}
