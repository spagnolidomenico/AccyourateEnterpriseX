namespace Accyourate.App.UIFramework.AI;

public sealed class AiAssistantIntent
{
    public string Query { get; init; } = "";
    public string Category { get; init; } = "";
    public string SuggestedAction { get; init; } = "";
    public string Explanation { get; init; } = "";
}
