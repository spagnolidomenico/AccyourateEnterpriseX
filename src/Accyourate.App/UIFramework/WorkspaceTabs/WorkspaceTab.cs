using Avalonia.Controls;

namespace Accyourate.App.UIFramework.WorkspaceTabs;

public sealed class WorkspaceTab
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Icon { get; init; } = string.Empty;
    public Control Content { get; init; } = new TextBlock { Text = "Empty tab" };
    public bool CanClose { get; init; } = true;
    public bool IsPinned { get; init; }
    public DateTime OpenedAt { get; init; } = DateTime.Now;
}
