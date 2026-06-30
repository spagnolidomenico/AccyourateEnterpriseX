namespace Accyourate.App.ActionEngine;

public sealed class ActionRequest
{
    public string ActionId { get; init; } = string.Empty;
    public string ModuleId { get; init; } = string.Empty;
    public string Query { get; init; } = string.Empty;
    public Dictionary<string, string> Parameters { get; init; } = new();
}
