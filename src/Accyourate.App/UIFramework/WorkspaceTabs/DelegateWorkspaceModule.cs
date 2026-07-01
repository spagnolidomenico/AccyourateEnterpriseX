using Avalonia.Controls;

namespace Accyourate.App.UIFramework.WorkspaceTabs;

public sealed class DelegateWorkspaceModule : IWorkspaceModule
{
    private readonly Func<Control> _createView;

    public DelegateWorkspaceModule(
        string id,
        string title,
        string icon,
        bool canClose,
        bool isPinned,
        Func<Control> createView)
    {
        Id = id;
        Title = title;
        Icon = icon;
        CanClose = canClose;
        IsPinned = isPinned;
        _createView = createView;
    }

    public string Id { get; }
    public string Title { get; }
    public string Icon { get; }
    public bool CanClose { get; }
    public bool IsPinned { get; }

    public Control CreateView() => _createView();
}
