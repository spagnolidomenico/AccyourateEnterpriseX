using Avalonia.Controls;
using Accyourate.App.Platform.Search;

namespace Accyourate.App.UIFramework.WorkspaceModules;

public sealed class EnterpriseSearchWorkspaceModule : IWorkspaceModule
{
    private readonly Action<string, string>? _navigate;

    public EnterpriseSearchWorkspaceModule(Action<string, string>? navigate = null)
    {
        _navigate = navigate;
    }

    public string Id => "enterprise-search";
    public string Title => "Ricerca Enterprise";
    public string Icon => "🔎";
    public bool CanClose => true;
    public bool IsPinned => false;

    public Control CreateView() => new EnterpriseSearchView(_navigate);
}
