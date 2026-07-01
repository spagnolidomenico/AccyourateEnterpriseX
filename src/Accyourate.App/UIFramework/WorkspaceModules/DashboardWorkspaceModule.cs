using Avalonia.Controls;
using Accyourate.App.UIFramework.Icons;
using Accyourate.App.UIFramework.Shell;

namespace Accyourate.App.UIFramework.WorkspaceModules;

public sealed class DashboardWorkspaceModule : IWorkspaceModule
{
    private readonly WorkspaceModuleFactory _moduleFactory;

    public DashboardWorkspaceModule(WorkspaceModuleFactory moduleFactory)
    {
        _moduleFactory = moduleFactory;
    }

    public string Id => "dashboard";
    public string Title => "Dashboard";
    public string Icon => AxIcons.Dashboard;
    public bool CanClose => false;
    public bool IsPinned => true;

    public Control CreateView()
    {
        return _moduleFactory.Create(Id);
    }
}
