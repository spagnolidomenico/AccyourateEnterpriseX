namespace Accyourate.App.UIFramework.WorkspaceModules;

public sealed class WorkspaceModuleDescriptor
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Icon { get; init; } = string.Empty;
    public bool CanClose { get; init; } = true;
    public bool IsPinned { get; init; }
}
