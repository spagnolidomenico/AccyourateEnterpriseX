using Avalonia.Controls;
using Accyourate.App.Platform.Update;

namespace Accyourate.App.UIFramework.WorkspaceModules;

public sealed class UpdateWorkspaceModule : IWorkspaceModule
{
    public string Id => "update-center";
    public string Title => "Update Center";
    public string Icon => "🔄";
    public bool CanClose => true;
    public bool IsPinned => false;
    public Control CreateView() => new UpdateCenterView();
}
