using Avalonia.Controls;
using Accyourate.App.Data;
using Accyourate.App.Models;

namespace Accyourate.App.UIFramework.WorkspaceModules;

public sealed class ActionEngineWorkspaceModule : IWorkspaceModule
{
    private readonly DatabaseService _database;
    private readonly CurrentUser _user;

    public ActionEngineWorkspaceModule(DatabaseService database, CurrentUser user)
    {
        _database = database;
        _user = user;
    }

    public string Id => "action-engine";
    public string Title => "Action Engine";
    public string Icon => "AX";
    public bool CanClose => true;
    public bool IsPinned => false;

    public Control CreateView()
    {
        return new ActionEngineView(_database, _user);
    }
}
