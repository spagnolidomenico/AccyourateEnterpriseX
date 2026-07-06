namespace Accyourate.App.Platform.About;

public sealed class AboutSystemInfo
{
    public string ProductName { get; set; } = "Accyourate Enterprise X";
    public string Version { get; set; } = "0.9.0-beta";
    public string Build { get; set; } = "Local";
    public string Framework { get; set; } = string.Empty;
    public string OperatingSystem { get; set; } = string.Empty;
    public string Architecture { get; set; } = string.Empty;
    public string AppDataFolder { get; set; } = string.Empty;
    public string DocumentFolder { get; set; } = string.Empty;
    public string DatabaseSummary { get; set; } = string.Empty;
    public string LicenseEdition { get; set; } = "Enterprise Beta";
}
