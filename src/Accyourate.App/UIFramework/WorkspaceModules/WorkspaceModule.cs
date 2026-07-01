using Avalonia.Controls;

namespace Accyourate.App.UIFramework.WorkspaceModules;

public sealed class WorkspaceModule : IWorkspaceModule
{
    private readonly Func<Control> _factory;

    public WorkspaceModule(
        string id,
        string title,
        string icon,
        Func<Control> factory,
        bool canClose = true,
        bool isPinned = false)
    {
        Id = id;
        Title = title;
        Icon = icon;
        _factory = factory;
        CanClose = canClose;
        IsPinned = isPinned;
    }

    public string Id { get; }
    public string Title { get; }
    public string Icon { get; }
    public bool CanClose { get; }
    public bool IsPinned { get; }

    public Control CreateView()
    {
        return _factory();
    }
}
