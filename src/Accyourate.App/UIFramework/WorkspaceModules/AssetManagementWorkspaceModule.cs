using Avalonia.Controls;
using Accyourate.App.AssetManagement;

namespace Accyourate.App.UIFramework.WorkspaceModules;

public sealed class AssetManagementWorkspaceModule : IWorkspaceModule
{
    public string Id => "asset-management";
    public string Title => "Asset Management";
    public string Icon => "IT";
    public bool CanClose => true;
    public bool IsPinned => false;

    public Control CreateView()
    {
        return new AssetManagementView();
    }
}
