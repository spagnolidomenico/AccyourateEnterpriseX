using System.Runtime.InteropServices;
using Accyourate.App.Platform.Settings;

namespace Accyourate.App.Platform.About;

public sealed class AboutService
{
    private readonly SettingsService _settingsService;

    public AboutService(SettingsService? settingsService = null)
    {
        _settingsService = settingsService ?? new SettingsService();
    }

    public AboutSystemInfo GetSystemInfo()
    {
        var settings = _settingsService.Load();
        var appFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AccyourateEnterpriseX");

        return new AboutSystemInfo
        {
            ProductName = "Accyourate Enterprise X",
            Version = "0.9.0-beta",
            Build = DateTime.Now.ToString("yyyyMMdd"),
            Framework = RuntimeInformation.FrameworkDescription,
            OperatingSystem = RuntimeInformation.OSDescription,
            Architecture = RuntimeInformation.ProcessArchitecture.ToString(),
            AppDataFolder = appFolder,
            DocumentFolder = settings.Documents.DocumentRootPath,
            DatabaseSummary = DatabaseSummary(appFolder),
            LicenseEdition = "Enterprise Beta"
        };
    }

    public IReadOnlyList<AboutModuleInfo> GetModules()
    {
        return new List<AboutModuleInfo>
        {
            new() { Name = "Workspace Enterprise", Status = "Disponibile", Version = "Beta", Description = "Shell modulare a tab." },
            new() { Name = "Human Resources", Status = "Disponibile", Version = "Beta", Description = "Gestione dipendenti e profili." },
            new() { Name = "Asset Management", Status = "Disponibile", Version = "Beta", Description = "Inventario, assegnazioni e restituzioni." },
            new() { Name = "Delivery Reports", Status = "Disponibile", Version = "Beta", Description = "Verbali di consegna e PDF." },
            new() { Name = "Settings Center", Status = "Disponibile", Version = "Beta", Description = "Configurazione azienda, numerazioni e percorsi." },
            new() { Name = "Document Center", Status = "Disponibile", Version = "Beta", Description = "Archivio documentale centrale." },
            new() { Name = "Enterprise Dashboard", Status = "Disponibile", Version = "Beta", Description = "KPI di piattaforma." },
            new() { Name = "Enterprise Search", Status = "Disponibile", Version = "Beta", Description = "Ricerca globale." },
            new() { Name = "Notification Center", Status = "Disponibile", Version = "Beta", Description = "Notifiche di sistema." },
            new() { Name = "Audit Engine", Status = "Disponibile", Version = "Beta", Description = "Tracciamento attività." },
            new() { Name = "Update Center", Status = "Pianificato", Version = "Future", Description = "Verifica aggiornamenti e changelog." },
            new() { Name = "Cloud Sync", Status = "Pianificato", Version = "Future", Description = "Sincronizzazione multi-postazione." }
        };
    }

    private static string DatabaseSummary(string appFolder)
    {
        var files = new[]
        {
            "accyourate-hr.db",
            "accyourate-assets.db",
            "accyourate-platform.db",
            "settings.json"
        };

        return string.Join(Environment.NewLine, files.Select(file =>
        {
            var path = Path.Combine(appFolder, file);
            if (!File.Exists(path))
                return $"{file}: non presente";

            var size = new FileInfo(path).Length;
            return $"{file}: {FormatSize(size)}";
        }));
    }

    private static string FormatSize(long bytes)
    {
        if (bytes < 1024)
            return $"{bytes} B";
        if (bytes < 1024 * 1024)
            return $"{bytes / 1024.0:0.0} KB";
        return $"{bytes / 1024.0 / 1024.0:0.0} MB";
    }
}
