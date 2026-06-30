using Accyourate.App.DigitalTwin;

namespace Accyourate.App.ActionEngine.DigitalTwin;

public static class DigitalTwinCapabilities
{
    public static void RegisterAll(CapabilityRegistry registry)
    {
        registry.Register(new SearchDeviceCapability());
        registry.Register(new OpenDeviceCapability());
        registry.Register(new FilterLowBatteryCapability());
        registry.Register(new FilterOfflineCapability());
        registry.Register(new ShowTelemetryCapability());
        registry.Register(new ShowEcgCapability());
    }
}

public sealed class SearchDeviceCapability : IActionCapabilityHandler
{
    public CapabilityDescriptor Descriptor { get; } = new()
    {
        Id = "digital-twin.search-device",
        ModuleId = "digital-twin",
        DisplayName = "Search Digital Twin Device",
        Description = "Cerca un dispositivo Digital Twin.",
        RequiredPermission = "read"
    };

    public ActionResult Execute(ActionRequest request, ActionContext context)
    {
        var service = new DigitalTwinService(context.Database);
        var code = request.Parameters.TryGetValue("deviceCode", out var c) ? c : "";
        var devices = service.GetDevices();

        if (!string.IsNullOrWhiteSpace(code))
        {
            var found = devices.FirstOrDefault(d => d.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            return found is null
                ? ActionResult.Fail($"Dispositivo non trovato: {code}")
                : ActionResult.Ok($"Dispositivo trovato: {found.Code} · {found.Model} · Stato {found.Status}", "digital-twin", "digital-twin");
        }

        return ActionResult.Ok($"Dispositivi Digital Twin disponibili: {devices.Count}.", "digital-twin", "digital-twin");
    }
}

public sealed class OpenDeviceCapability : IActionCapabilityHandler
{
    public CapabilityDescriptor Descriptor { get; } = new()
    {
        Id = "digital-twin.open-device",
        ModuleId = "digital-twin",
        DisplayName = "Open Digital Twin Device",
        Description = "Apre il modulo Digital Twin e prepara la selezione del dispositivo.",
        RequiredPermission = "read"
    };

    public ActionResult Execute(ActionRequest request, ActionContext context)
    {
        var code = request.Parameters.TryGetValue("deviceCode", out var c) ? c : "";
        if (string.IsNullOrWhiteSpace(code))
            return ActionResult.Ok("Apro il modulo Digital Twin. Nessun codice dispositivo specificato.", "digital-twin", "digital-twin");

        var service = new DigitalTwinService(context.Database);
        var found = service.GetDevices().FirstOrDefault(d => d.Code.Equals(code, StringComparison.OrdinalIgnoreCase));

        return found is null
            ? ActionResult.Fail($"Non ho trovato il dispositivo {code}.")
            : ActionResult.Ok($"Apro Digital Twin per {found.Code} · {found.Model}.", "digital-twin", $"digital-twin:{found.Code}");
    }
}

public sealed class FilterLowBatteryCapability : IActionCapabilityHandler
{
    public CapabilityDescriptor Descriptor { get; } = new()
    {
        Id = "digital-twin.filter-low-battery",
        ModuleId = "digital-twin",
        DisplayName = "Filter Low Battery Devices",
        Description = "Filtra i Digital Twin con batteria sotto soglia.",
        RequiredPermission = "read"
    };

    public ActionResult Execute(ActionRequest request, ActionContext context)
    {
        var thresholdText = request.Parameters.TryGetValue("threshold", out var t) ? t : "20";
        var threshold = int.TryParse(thresholdText, out var parsed) ? parsed : 20;

        var service = new DigitalTwinService(context.Database);
        var count = service.GetDevices().Count(d => d.BatteryLevel < threshold);

        return ActionResult.Ok($"Dispositivi con batteria sotto {threshold}%: {count}.", "digital-twin", $"digital-twin:low-battery:{threshold}");
    }
}

public sealed class FilterOfflineCapability : IActionCapabilityHandler
{
    public CapabilityDescriptor Descriptor { get; } = new()
    {
        Id = "digital-twin.filter-offline",
        ModuleId = "digital-twin",
        DisplayName = "Filter Offline Devices",
        Description = "Mostra i Digital Twin offline.",
        RequiredPermission = "read"
    };

    public ActionResult Execute(ActionRequest request, ActionContext context)
    {
        var service = new DigitalTwinService(context.Database);
        var count = service.GetDevices().Count(d => d.Status.Equals("Offline", StringComparison.OrdinalIgnoreCase));

        return ActionResult.Ok($"Dispositivi Digital Twin offline: {count}.", "digital-twin", "digital-twin:offline");
    }
}

public sealed class ShowTelemetryCapability : IActionCapabilityHandler
{
    public CapabilityDescriptor Descriptor { get; } = new()
    {
        Id = "digital-twin.show-telemetry",
        ModuleId = "digital-twin",
        DisplayName = "Show Telemetry",
        Description = "Mostra la telemetria del Digital Twin.",
        RequiredPermission = "read"
    };

    public ActionResult Execute(ActionRequest request, ActionContext context)
    {
        var service = new DigitalTwinService(context.Database);
        var telemetry = service.GetTelemetry();
        return ActionResult.Ok($"Eventi telemetria disponibili: {telemetry.Count}.", "digital-twin", "digital-twin:telemetry");
    }
}

public sealed class ShowEcgCapability : IActionCapabilityHandler
{
    public CapabilityDescriptor Descriptor { get; } = new()
    {
        Id = "digital-twin.show-ecg",
        ModuleId = "digital-twin",
        DisplayName = "Show ECG",
        Description = "Mostra i dati ECG disponibili.",
        RequiredPermission = "read"
    };

    public ActionResult Execute(ActionRequest request, ActionContext context)
    {
        var service = new DigitalTwinService(context.Database);
        var normal = service.CountEcgNormal();
        return ActionResult.Ok($"Dispositivi con ECG normale nell'ultima lettura: {normal}.", "digital-twin", "digital-twin:ecg");
    }
}
