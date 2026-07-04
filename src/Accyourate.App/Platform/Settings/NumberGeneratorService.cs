namespace Accyourate.App.Platform.Settings;

public sealed class NumberGeneratorService
{
    private readonly SettingsService _settingsService;

    public NumberGeneratorService(SettingsService? settingsService = null)
    {
        _settingsService = settingsService ?? new SettingsService();
    }

    public string DeliveryReportNumber(int sequence)
    {
        return _settingsService.FormatDeliveryReportNumber(sequence);
    }

    public string EmployeeCode(int sequence)
    {
        var settings = _settingsService.Load();
        return Format(settings.Numbering.EmployeePrefix, sequence, settings.Numbering.Padding, false);
    }

    public string AssetCode(int sequence)
    {
        var settings = _settingsService.Load();
        return Format(settings.Numbering.AssetPrefix, sequence, settings.Numbering.Padding, false);
    }

    public string DocumentNumber(int sequence)
    {
        var settings = _settingsService.Load();
        return Format(settings.Numbering.DocumentPrefix, sequence, settings.Numbering.Padding, false);
    }

    private static string Format(string prefix, int sequence, int padding, bool includeYear)
    {
        prefix = string.IsNullOrWhiteSpace(prefix) ? "ACC" : prefix.Trim().ToUpperInvariant();
        padding = Math.Clamp(padding, 3, 10);
        var number = sequence.ToString($"D{padding}");
        return includeYear ? $"{prefix}-{DateTime.Now:yyyy}-{number}" : $"{prefix}-{number}";
    }
}
