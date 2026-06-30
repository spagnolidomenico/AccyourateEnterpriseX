namespace Accyourate.App.UIFramework.AI;

public sealed class AiDataQueryResult
{
    public string Entity { get; init; } = string.Empty;
    public int Count { get; init; }
    public string Summary { get; init; } = string.Empty;
}
