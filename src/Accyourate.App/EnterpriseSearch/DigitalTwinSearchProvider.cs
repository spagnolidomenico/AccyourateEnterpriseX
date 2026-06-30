using Accyourate.App.DigitalTwin;

namespace Accyourate.App.EnterpriseSearch;

public sealed class DigitalTwinSearchProvider : ISearchProvider
{
    private readonly DigitalTwinService _service;

    public DigitalTwinSearchProvider(DigitalTwinService service)
    {
        _service = service;
    }

    public string ProviderId => "digital-twin";
    public string DisplayName => "Digital Twin";

    public IEnumerable<SearchResult> Search(string query)
    {
        var q = (query ?? string.Empty).Trim().ToLowerInvariant();

        foreach (var device in _service.GetDevices())
        {
            var haystack = $"{device.Code} {device.Type} {device.Model} {device.SerialNumber} {device.Rfid} {device.Status} {device.EcgStatus} {device.SignalQuality} {device.AssignedTo}".ToLowerInvariant();

            if (haystack.Contains(q))
            {
                yield return new SearchResult
                {
                    Title = device.Code,
                    Subtitle = $"{device.Model} · {device.Status} · Batteria {device.BatteryLevel}% · ECG {device.EcgStatus}",
                    Type = "Digital Twin",
                    Icon = "DT",
                    ModuleId = "digital-twin",
                    ActionId = "digital-twin.open-device",
                    Parameters = new Dictionary<string, string> { ["deviceCode"] = device.Code }
                };
            }
        }

        if ("offline".Contains(q) || q.Contains("offline"))
        {
            yield return new SearchResult
            {
                Title = "Dispositivi offline",
                Subtitle = "Mostra tutti i Digital Twin offline",
                Type = "Azione",
                Icon = "○",
                ModuleId = "digital-twin",
                ActionId = "digital-twin.filter-offline"
            };
        }

        if (q.Contains("batteria") || q.Contains("battery") || q.Contains("bassa"))
        {
            yield return new SearchResult
            {
                Title = "Batteria sotto 20%",
                Subtitle = "Mostra Digital Twin con batteria bassa",
                Type = "Azione",
                Icon = "!",
                ModuleId = "digital-twin",
                ActionId = "digital-twin.filter-low-battery",
                Parameters = new Dictionary<string, string> { ["threshold"] = "20" }
            };
        }

        if (q.Contains("ecg") || q.Contains("elettrocardiogramma"))
        {
            yield return new SearchResult
            {
                Title = "ECG Digital Twin",
                Subtitle = "Mostra dati ECG disponibili",
                Type = "Azione",
                Icon = "♡",
                ModuleId = "digital-twin",
                ActionId = "digital-twin.show-ecg"
            };
        }

        if (q.Contains("telemetria") || q.Contains("telemetry"))
        {
            yield return new SearchResult
            {
                Title = "Telemetria Digital Twin",
                Subtitle = "Mostra eventi telemetria",
                Type = "Azione",
                Icon = "⌁",
                ModuleId = "digital-twin",
                ActionId = "digital-twin.show-telemetry"
            };
        }
    }
}
