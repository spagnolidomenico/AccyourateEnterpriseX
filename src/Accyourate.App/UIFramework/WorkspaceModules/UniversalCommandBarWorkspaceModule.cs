using Avalonia.Controls;
using Accyourate.App.Data;
using Accyourate.App.Models;

namespace Accyourate.App.UIFramework.WorkspaceModules;

public sealed class UniversalCommandBarWorkspaceModule : IWorkspaceModule
{
    private readonly DatabaseService _database;
    private readonly CurrentUser _user;
    private readonly Action<string, string>? _navigate;

    public UniversalCommandBarWorkspaceModule(DatabaseService database, CurrentUser user, Action<string, string>? navigate)
    {
        _database = database;
        _user = user;
        _navigate = navigate;
    }

    public string Id => "universal-command-bar";
    public string Title => "Universal Command Bar";
    public string Icon => "⌕";
    public bool CanClose => true;
    public bool IsPinned => false;

    public Control CreateView()
    {
        return new UniversalCommandBarView(_database, _user, _navigate);
    }
}
