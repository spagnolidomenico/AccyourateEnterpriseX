using Accyourate.App.UIFramework.Icons;

namespace Accyourate.App.UIFramework.Widgets;

public static class WorkspaceWidgetRegistry
{
    public static IReadOnlyList<WorkspaceWidgetDescriptor> Widgets { get; } = new List<WorkspaceWidgetDescriptor>
    {
        new() { Id = "kpi-medical", Title = "Dispositivi medici", Category = "KPI", Icon = AxIcons.Medical },
        new() { Id = "kpi-documents", Title = "Documenti", Category = "KPI", Icon = AxIcons.Documents },
        new() { Id = "kpi-assets", Title = "Asset IT", Category = "KPI", Icon = AxIcons.Assets },
        new() { Id = "kpi-people", Title = "Persone", Category = "KPI", Icon = AxIcons.People },
        new() { Id = "system-status", Title = "Stato sistemi", Category = "Operations", Icon = AxIcons.Status, DefaultWidth = 2 },
        new() { Id = "recent-activity", Title = "Attività recenti", Category = "Operations", Icon = "◷", DefaultWidth = 2 },
        new() { Id = "deadlines", Title = "Scadenze", Category = "Operations", Icon = "!" },
        new() { Id = "quick-actions", Title = "Accessi rapidi", Category = "Navigation", Icon = AxIcons.Command },
        new() { Id = "medical-lifecycle", Title = "Lifecycle Medical", Category = "Medical", Icon = AxIcons.Medical, DefaultWidth = 2 },
        new() { Id = "analytics-trend", Title = "Trend operativo", Category = "Analytics", Icon = AxIcons.Analytics, DefaultWidth = 2 }
    };
}
