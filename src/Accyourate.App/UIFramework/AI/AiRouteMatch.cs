namespace Accyourate.App.UIFramework.AI;

public sealed class AiRouteMatch
{
    public AiIntentDefinition Intent { get; init; } = new();
    public int Score { get; init; }
    public string[] MatchedTerms { get; init; } = Array.Empty<string>();
}
