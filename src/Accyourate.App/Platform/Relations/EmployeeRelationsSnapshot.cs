namespace Accyourate.App.Platform.Relations;

public sealed class EmployeeRelationsSnapshot
{
    public string EmployeeId { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public IReadOnlyList<EnterpriseRelationItem> Assets { get; set; } = Array.Empty<EnterpriseRelationItem>();
    public IReadOnlyList<EnterpriseRelationItem> Documents { get; set; } = Array.Empty<EnterpriseRelationItem>();
    public IReadOnlyList<EnterpriseRelationItem> DeliveryReports { get; set; } = Array.Empty<EnterpriseRelationItem>();
    public int TotalRelations => Assets.Count + Documents.Count + DeliveryReports.Count;
}
