namespace Accyourate.App.Platform.Update;

public sealed class VersionManifest
{
    public string Product { get; set; } = "Accyourate Enterprise X";
    public string InstalledVersion { get; set; } = "0.9.0-beta";
    public string LatestVersion { get; set; } = "0.9.0-beta";
    public string Channel { get; set; } = "Beta";
    public string Status { get; set; } = "Aggiornato";
    public string ReleaseDate { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");
    public string DownloadUrl { get; set; } = string.Empty;
    public string Notes { get; set; } = "Update Center predisposto per aggiornamenti online.";
}
