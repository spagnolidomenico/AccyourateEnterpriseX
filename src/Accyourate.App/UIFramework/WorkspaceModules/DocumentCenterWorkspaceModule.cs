using Avalonia.Controls;
using Accyourate.App.Platform.Documents;

namespace Accyourate.App.UIFramework.WorkspaceModules;

public sealed class DocumentCenterWorkspaceModule : IWorkspaceModule
{
    public string Id => "document-center";
    public string Title => "Centro Documenti";
    public string Icon => "📁";
    public bool CanClose => true;
    public bool IsPinned => false;

    public Control CreateView()
    {
        return new DocumentCenterView();
    }
}
