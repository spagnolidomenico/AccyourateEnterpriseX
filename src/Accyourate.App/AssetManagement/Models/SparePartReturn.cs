namespace Accyourate.App.AssetManagement.Models;

public static class SparePartReturnCondition
{
    public const string Reusable="Riutilizzabile";
    public const string Damaged="Danneggiato";
    public const string Discarded="Da smaltire";
}

public sealed class SparePartReturn
{
    public int Id { get; set; }
    public string ReturnNumber { get; set; } = string.Empty;
    public int PickRequestId { get; set; }
    public int InventoryItemId { get; set; }
    public int LocationId { get; set; }
    public decimal Quantity { get; set; }
    public string Condition { get; set; } = SparePartReturnCondition.Reusable;
    public string Reason { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string OperatorName { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = DateTime.Now.ToString("s");
}
