namespace Accyourate.App.UIFramework.Widgets;

public sealed class WorkspaceWidgetLayout
{
    public string UserName { get; set; } = "admin";
    public List<string> VisibleWidgetIds { get; set; } = new()
    {
        "kpi-medical",
        "kpi-documents",
        "kpi-assets",
        "kpi-people",
        "system-status",
        "recent-activity",
        "deadlines",
        "quick-actions"
    };
}
