namespace Accyourate.App.UIFramework.DesignSystem;

/// <summary>
/// Registro descrittivo dei componenti canonici del Design System.
/// Serve come riferimento unico per evitare controlli duplicati nei moduli.
/// </summary>
public static class AxComponentCatalog
{
    public const string Version = "6.1";

    public static IReadOnlyList<string> Components { get; } = new[]
    {
        "AxButton",
        "AxCommandButton",
        "AxToolbar",
        "AxCard",
        "AxKpiCard",
        "AxSearchBox",
        "AxStatusBadge",
        "AxEnterpriseTable",
        "AxInspectorPanel",
        "AxTimeline",
        "AxEmptyState"
    };
}
