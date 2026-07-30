namespace Accyourate.App.AssetManagement.Models;

public static class ReplenishmentRequestStatus
{
    public const string Draft = "Bozza";
    public const string Approved = "Approvata";
    public const string Ordered = "Ordinata";
    public const string Completed = "Completata";
    public const string Cancelled = "Annullata";
}

public sealed class SparePartReplenishmentRequest
{
    public int Id { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public int InventoryItemId { get; set; }
    public int SupplierId { get; set; }
    public string Status { get; set; } = ReplenishmentRequestStatus.Draft;
    public decimal SuggestedQuantity { get; set; }
    public decimal RequestedQuantity { get; set; }
    public string Notes { get; set; } = string.Empty;
    public int PurchaseOrderId { get; set; }
    public string CreatedAt { get; set; } = DateTime.Now.ToString("s");
    public string UpdatedAt { get; set; } = DateTime.Now.ToString("s");
}
