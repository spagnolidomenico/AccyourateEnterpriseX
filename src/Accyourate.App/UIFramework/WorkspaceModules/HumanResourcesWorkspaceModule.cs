using Avalonia.Controls;
using Accyourate.App.HumanResources;

namespace Accyourate.App.UIFramework.WorkspaceModules;

public sealed class HumanResourcesWorkspaceModule : IWorkspaceModule
{
    public string Id => "human-resources";
    public string Title => "Human Resources";
    public string Icon => "👥";
    public bool CanClose => true;
    public bool IsPinned => false;

    public Control CreateView()
    {
        return new HumanResourcesView();
    }
}
