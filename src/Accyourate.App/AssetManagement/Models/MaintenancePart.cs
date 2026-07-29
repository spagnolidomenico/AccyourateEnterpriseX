namespace Accyourate.App.AssetManagement.Models;

public sealed class MaintenancePart
{
    public int Id { get; set; }
    public int MaintenanceTicketId { get; set; }
    public string PartCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Supplier { get; set; } = string.Empty;
    public decimal Quantity { get; set; } = 1;
    public decimal UnitCost { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = DateTime.Now.ToString("s");
    public decimal TotalCost => Quantity * UnitCost;
}

public sealed class MaintenanceSupplier
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string VatNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
