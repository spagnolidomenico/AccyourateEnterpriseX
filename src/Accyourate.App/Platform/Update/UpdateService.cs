using System.Text.Json;
using Accyourate.App.Platform.Settings;

namespace Accyourate.App.Platform.Update;

public sealed class UpdateService
{
    private readonly SettingsService _settingsService;

    public UpdateService(SettingsService? settingsService = null)
    {
        _settingsService = settingsService ?? new SettingsService();
    }

    public VersionManifest GetInstalledManifest()
    {
        var settings = _settingsService.Load();
        return new VersionManifest
        {
            Product = "Accyourate Enterprise X",
            InstalledVersion = "0.9.0-beta",
            LatestVersion = "0.9.0-beta",
            Channel = string.IsNullOrWhiteSpace(settings.VersionChannel) ? "Beta" : settings.VersionChannel,
            Status = "Aggiornato",
            ReleaseDate = DateTime.Now.ToString("yyyy-MM-dd"),
            Notes = "Versione locale Beta. La verifica online verrà collegata a un endpoint nella fase successiva."
        };
    }

    public VersionManifest CheckForUpdates()
    {
        var manifest = GetInstalledManifest();
        manifest.Status = "Nessun aggiornamento disponibile";
        manifest.Notes = "Verifica simulata completata. Il modulo è pronto per integrazione GitHub/API.";
        return manifest;
    }

    public IReadOnlyList<ReleaseNote> GetReleaseNotes()
    {
        return new List<ReleaseNote>
        {
            new() { Version = "0.9.0-beta", Date = DateTime.Now.ToString("yyyy-MM-dd"), Title = "Beta foundation", Notes = "Workspace, HR, Asset Management, Delivery Reports, Settings, Document Center, Dashboard, Search, Backup e About Center." },
            new() { Version = "0.8.x", Date = "2026-07", Title = "Enterprise platform", Notes = "Servizi trasversali: Audit, Notification Center, PDF Engine e repository SQLite." }
        };
    }

    public string ExportLocalManifest()
    {
        var appFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AccyourateEnterpriseX");
        Directory.CreateDirectory(appFolder);
        var path = Path.Combine(appFolder, "version-manifest.json");
        File.WriteAllText(path, JsonSerializer.Serialize(GetInstalledManifest(), new JsonSerializerOptions { WriteIndented = true }));
        return path;
    }
}
