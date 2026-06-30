using Accyourate.App.Api.Contracts;

namespace Accyourate.App.Api;

public static class ApiFoundationRegistry
{
    public static IReadOnlyList<ApiEndpointDescriptor> PlannedEndpoints { get; } = new List<ApiEndpointDescriptor>
    {
        new() { Method = "GET", Route = "/api/people", Description = "Elenco persone" },
        new() { Method = "GET", Route = "/api/assets", Description = "Elenco asset IT" },
        new() { Method = "GET", Route = "/api/medical/devices", Description = "Elenco dispositivi medici" },
        new() { Method = "GET", Route = "/api/documents", Description = "Elenco documenti" },
        new() { Method = "GET", Route = "/api/workflow/events", Description = "Eventi Digital Twin" }
    };
}
