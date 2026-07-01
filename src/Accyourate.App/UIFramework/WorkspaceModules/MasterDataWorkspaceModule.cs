using Avalonia.Controls;
using Accyourate.App.EnterpriseMasterData;

namespace Accyourate.App.UIFramework.WorkspaceModules;

public sealed class MasterDataWorkspaceModule : IWorkspaceModule
{
    public string Id => "master-data";
    public string Title => "Anagrafica Aziendale";
    public string Icon => "🏢";
    public bool CanClose => true;
    public bool IsPinned => false;

    public Control CreateView()
    {
        return new MasterDataView();
    }
}
