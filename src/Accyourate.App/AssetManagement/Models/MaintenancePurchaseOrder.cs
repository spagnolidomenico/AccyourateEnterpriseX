namespace Accyourate.App.AssetManagement.Models;

public static class PurchaseOrderStatus
{
    public const string Draft = "Bozza";
    public const string Sent = "Inviato";
    public const string Confirmed = "Confermato";
    public const string Received = "Ricevuto";
    public const string Cancelled = "Annullato";
}

public sealed class MaintenancePurchaseOrder
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public int MaintenanceTicketId { get; set; }
    public string Status { get; set; } = PurchaseOrderStatus.Draft;
    public string OrderDate { get; set; } = DateTime.Today.ToString("s");
    public string ExpectedDate { get; set; } = string.Empty;
    public string ReceivedDate { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string PdfPath { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = DateTime.Now.ToString("s");
    public string UpdatedAt { get; set; } = DateTime.Now.ToString("s");
    public List<MaintenancePurchaseOrderLine> Lines { get; set; } = new();
    public decimal Total => Lines.Sum(line => line.Total);
}

public sealed class MaintenancePurchaseOrderLine
{
    public int Id { get; set; }
    public int PurchaseOrderId { get; set; }
    public string PartCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; } = 1;
    public decimal UnitCost { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public decimal Total => Quantity * UnitCost;
}
