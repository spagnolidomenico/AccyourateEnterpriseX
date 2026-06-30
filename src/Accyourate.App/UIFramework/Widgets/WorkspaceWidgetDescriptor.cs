namespace Accyourate.App.UIFramework.Widgets;

public sealed class WorkspaceWidgetDescriptor
{
    public string Id { get; init; } = "";
    public string Title { get; init; } = "";
    public string Category { get; init; } = "";
    public string Icon { get; init; } = "";
    public int DefaultWidth { get; init; } = 1;
    public int DefaultHeight { get; init; } = 1;
}
