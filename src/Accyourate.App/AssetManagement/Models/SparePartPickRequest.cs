namespace Accyourate.App.AssetManagement.Models;

public static class SparePartPickRequestStatus
{
    public const string Draft="Bozza";
    public const string Approved="Approvata";
    public const string Preparing="In preparazione";
    public const string Delivered="Consegnata";
    public const string Cancelled="Annullata";
}

public sealed class SparePartPickRequest
{
    public int Id { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public int InventoryItemId { get; set; }
    public decimal Quantity { get; set; }
    public int PreferredLocationId { get; set; }
    public int MaintenanceTicketId { get; set; }
    public string RequestedBy { get; set; } = string.Empty;
    public string Technician { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string Status { get; set; } = SparePartPickRequestStatus.Draft;
    public string CreatedAt { get; set; } = DateTime.Now.ToString("s");
    public string UpdatedAt { get; set; } = DateTime.Now.ToString("s");
    public string DeliveredAt { get; set; } = string.Empty;
}
