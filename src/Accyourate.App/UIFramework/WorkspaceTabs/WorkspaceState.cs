namespace Accyourate.App.UIFramework.WorkspaceTabs;

public sealed class WorkspaceState
{
    public string ActiveTabId { get; init; } = string.Empty;
    public List<string> OpenTabIds { get; init; } = new();
    public DateTime UpdatedAt { get; init; } = DateTime.Now;
}
