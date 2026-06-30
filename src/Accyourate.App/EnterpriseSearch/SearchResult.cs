namespace Accyourate.App.EnterpriseSearch;

public sealed class SearchResult
{
    public string Title { get; init; } = string.Empty;
    public string Subtitle { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Icon { get; init; } = string.Empty;
    public string ModuleId { get; init; } = string.Empty;
    public string ActionId { get; init; } = string.Empty;
    public Dictionary<string, string> Parameters { get; init; } = new();
}
