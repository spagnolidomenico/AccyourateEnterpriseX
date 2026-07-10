namespace Accyourate.App.Platform.Relations;

public sealed class EnterpriseRelationItem
{
    public string Id { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string Icon { get; set; } = "🔗";
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string OpenModuleId { get; set; } = string.Empty;
    public string OpenModuleTitle { get; set; } = string.Empty;
}
