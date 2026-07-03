using Avalonia.Controls;
using Accyourate.App.Platform.Settings;

namespace Accyourate.App.UIFramework.WorkspaceModules;

public sealed class SettingsWorkspaceModule : IWorkspaceModule
{
    public string Id => "settings-center";
    public string Title => "Impostazioni";
    public string Icon => "⚙️";
    public bool CanClose => true;
    public bool IsPinned => false;

    public Control CreateView()
    {
        return new SettingsCenterView();
    }
}
