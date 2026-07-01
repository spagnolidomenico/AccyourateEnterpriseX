using Avalonia.Controls;
using Accyourate.App.Data;
using Accyourate.App.Models;

namespace Accyourate.App.UIFramework.WorkspaceModules;

public sealed class AiAssistantWorkspaceModule : IWorkspaceModule
{
    private readonly DatabaseService _database;
    private readonly CurrentUser _user;

    public AiAssistantWorkspaceModule(DatabaseService database, CurrentUser user)
    {
        _database = database;
        _user = user;
    }

    public string Id => "ai-assistant";
    public string Title => "AI Assistant";
    public string Icon => "AI";
    public bool CanClose => true;
    public bool IsPinned => false;

    public Control CreateView()
    {
        return new EnterpriseAiAssistantView(_database, _user);
    }
}
