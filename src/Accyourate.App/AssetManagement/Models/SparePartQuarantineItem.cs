namespace Accyourate.App.AssetManagement.Models;

public static class SparePartQuarantineStatus
{
    public const string Pending="Da valutare";
    public const string Repairable="Riparabile";
    public const string SupplierReturn="Da restituire al fornitore";
    public const string DisposalApproved="Smaltimento autorizzato";
    public const string Reintegrated="Reintegrato";
    public const string ReturnedToSupplier="Restituito al fornitore";
    public const string Disposed="Smaltito";
}

public sealed class SparePartQuarantineItem
{
    public int Id { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public int ReturnId { get; set; }
    public int InventoryItemId { get; set; }
    public int LocationId { get; set; }
    public decimal Quantity { get; set; }
    public string InitialCondition { get; set; } = string.Empty;
    public string Status { get; set; } = SparePartQuarantineStatus.Pending;
    public decimal EstimatedCost { get; set; }
    public string EvaluationNotes { get; set; } = string.Empty;
    public string AuthorizedBy { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = DateTime.Now.ToString("s");
    public string UpdatedAt { get; set; } = DateTime.Now.ToString("s");
    public string ClosedAt { get; set; } = string.Empty;
}
