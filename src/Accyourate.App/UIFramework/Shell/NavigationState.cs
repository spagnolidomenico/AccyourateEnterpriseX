namespace Accyourate.App.UIFramework.Shell;

public sealed class NavigationState
{
    public string CurrentModuleId { get; set; } = "workspace-home";
    public string CurrentTitle { get; set; } = "Workspace Home";
    public List<string> History { get; } = new();
}
