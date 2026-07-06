namespace Accyourate.App.Platform.Search;

public static class SearchCategory
{
    public const string HumanResources = "Human Resources";
    public const string Asset = "Asset";
    public const string DeliveryReport = "Verbali";
    public const string Document = "Documenti";
    public const string Notification = "Notifiche";
    public const string Audit = "Audit";
    public const string Settings = "Impostazioni";
}

public sealed class SearchRequest
{
    public string Query { get; set; } = string.Empty;
    public int Limit { get; set; } = 100;
}

public sealed class SearchResult
{
    public string Category { get; set; } = string.Empty;
    public string Icon { get; set; } = "⌕";
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string OpenAction { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
}
