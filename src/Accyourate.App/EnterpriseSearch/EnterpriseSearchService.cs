namespace Accyourate.App.EnterpriseSearch;

public sealed class EnterpriseSearchService
{
    private readonly List<ISearchProvider> _providers = new();

    public void Register(ISearchProvider provider)
    {
        if (_providers.Any(p => p.ProviderId == provider.ProviderId))
            return;

        _providers.Add(provider);
    }

    public IReadOnlyList<SearchResult> Search(string? query)
    {
        var q = query ?? string.Empty;

        if (string.IsNullOrWhiteSpace(q))
            return GetDefaultSuggestions();

        var results = new List<SearchResult>();

        foreach (var provider in _providers)
            results.AddRange(provider.Search(q));

        return results
            .OrderBy(r => r.Type)
            .ThenBy(r => r.Title)
            .Take(20)
            .ToList();
    }

    private IReadOnlyList<SearchResult> GetDefaultSuggestions()
    {
        return new List<SearchResult>
        {
            new()
            {
                Title = "Apri Digital Twin",
                Subtitle = "Vai alla piattaforma Digital Twin",
                Type = "Comando",
                Icon = "DT",
                ModuleId = "digital-twin",
                ActionId = "workspace.open-module",
                Parameters = new Dictionary<string, string> { ["module"] = "digital-twin" }
            },
            new()
            {
                Title = "Mostra dispositivi offline",
                Subtitle = "Filtra Digital Twin offline",
                Type = "Azione",
                Icon = "○",
                ModuleId = "digital-twin",
                ActionId = "digital-twin.filter-offline"
            },
            new()
            {
                Title = "Mostra batteria sotto 20%",
                Subtitle = "Filtra Digital Twin con batteria bassa",
                Type = "Azione",
                Icon = "!",
                ModuleId = "digital-twin",
                ActionId = "digital-twin.filter-low-battery",
                Parameters = new Dictionary<string, string> { ["threshold"] = "20" }
            },
            new()
            {
                Title = "Mostra telemetria",
                Subtitle = "Apri feed telemetria Digital Twin",
                Type = "Azione",
                Icon = "⌁",
                ModuleId = "digital-twin",
                ActionId = "digital-twin.show-telemetry"
            },
            new()
            {
                Title = "Mostra ECG",
                Subtitle = "Apri area ECG Digital Twin",
                Type = "Azione",
                Icon = "♡",
                ModuleId = "digital-twin",
                ActionId = "digital-twin.show-ecg"
            }
        };
    }
}
