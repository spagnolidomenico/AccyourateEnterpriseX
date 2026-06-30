namespace Accyourate.App.UIFramework.AI;

public sealed class AiIntentDefinition
{
    public string Id { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string SuggestedAction { get; init; } = string.Empty;
    public string ModuleId { get; init; } = string.Empty;
    public string[] Keywords { get; init; } = Array.Empty<string>();
    public string[] StrongKeywords { get; init; } = Array.Empty<string>();
}
