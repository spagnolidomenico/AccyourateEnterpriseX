using System.Text.Json;

namespace Accyourate.App.Platform.Settings;

public sealed class SettingsService
{
    private readonly string _settingsPath;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    public SettingsService(string? settingsPath = null)
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AccyourateEnterpriseX");

        Directory.CreateDirectory(folder);

        _settingsPath = settingsPath ?? Path.Combine(folder, "settings.json");
    }

    public string SettingsPath => _settingsPath;

    public ApplicationSettings Load()
    {
        if (!File.Exists(_settingsPath))
        {
            var defaults = new ApplicationSettings();
            Save(defaults);
            return defaults;
        }

        try
        {
            var json = File.ReadAllText(_settingsPath);
            return JsonSerializer.Deserialize<ApplicationSettings>(json, _jsonOptions) ?? new ApplicationSettings();
        }
        catch
        {
            var backup = $"{_settingsPath}.invalid-{DateTime.Now:yyyyMMddHHmmss}";
            File.Copy(_settingsPath, backup, true);

            var defaults = new ApplicationSettings();
            Save(defaults);
            return defaults;
        }
    }

    public void Save(ApplicationSettings settings)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath) ?? ".");
        var json = JsonSerializer.Serialize(settings, _jsonOptions);
        File.WriteAllText(_settingsPath, json);
    }

    public string GetDeliveryReportsFolder()
    {
        var settings = Load();
        var root = string.IsNullOrWhiteSpace(settings.Documents.DocumentRootPath)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Accyourate Enterprise X")
            : settings.Documents.DocumentRootPath;

        var folderName = string.IsNullOrWhiteSpace(settings.Documents.DeliveryReportsFolderName)
            ? "Verbali Consegna"
            : settings.Documents.DeliveryReportsFolderName;

        var path = Path.Combine(root, folderName);
        Directory.CreateDirectory(path);
        return path;
    }

    public string FormatDeliveryReportNumber(int sequence)
    {
        var settings = Load();
        var prefix = string.IsNullOrWhiteSpace(settings.Numbering.DeliveryReportPrefix)
            ? "VRB"
            : settings.Numbering.DeliveryReportPrefix;

        var padding = Math.Clamp(settings.Numbering.Padding, 3, 10);
        var number = sequence.ToString($"D{padding}");

        return settings.Numbering.IncludeYearInDeliveryReports
            ? $"{prefix}-{DateTime.Now:yyyy}-{number}"
            : $"{prefix}-{number}";
    }
}
