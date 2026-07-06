using Avalonia.Controls;
using Accyourate.App.Platform.About;

namespace Accyourate.App.UIFramework.WorkspaceModules;

public sealed class AboutWorkspaceModule : IWorkspaceModule
{
    public string Id => "about-center";
    public string Title => "Informazioni";
    public string Icon => "ℹ️";
    public bool CanClose => true;
    public bool IsPinned => false;

    public Control CreateView()
    {
        return new AboutCenterView();
    }
}
