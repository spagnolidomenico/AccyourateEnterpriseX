using Avalonia.Controls;
using Accyourate.App.UIFramework.Shell;

namespace Accyourate.App.UIFramework.WorkspaceModules;

public sealed class DigitalTwinWorkspaceModule : IWorkspaceModule
{
    private readonly WorkspaceModuleFactory _moduleFactory;

    public DigitalTwinWorkspaceModule(WorkspaceModuleFactory moduleFactory)
    {
        _moduleFactory = moduleFactory;
    }

    public string Id => "digital-twin";
    public string Title => "Digital Twin";
    public string Icon => "DT";
    public bool CanClose => true;
    public bool IsPinned => false;

    public Control CreateView()
    {
        return _moduleFactory.Create(Id);
    }
}
