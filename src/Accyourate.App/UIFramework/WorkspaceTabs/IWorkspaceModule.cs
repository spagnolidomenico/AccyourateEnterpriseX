using Avalonia.Controls;

namespace Accyourate.App.UIFramework.WorkspaceTabs;

public interface IWorkspaceModule
{
    string Id { get; }
    string Title { get; }
    string Icon { get; }
    bool CanClose { get; }
    bool IsPinned { get; }

    Control CreateView();
}
