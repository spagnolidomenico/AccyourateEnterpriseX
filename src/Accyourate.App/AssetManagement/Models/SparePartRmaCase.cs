namespace Accyourate.App.AssetManagement.Models;

public static class SparePartRmaStatus
{
    public const string Open="Aperta";
    public const string Authorized="Autorizzata";
    public const string Shipped="Spedita";
    public const string Resolved="Risolta";
    public const string Closed="Chiusa";
    public const string Cancelled="Annullata";
}
public static class SparePartRmaOutcome
{
    public const string Repair="Riparazione";
    public const string Replacement="Sostituzione";
    public const string Refund="Rimborso";
}
public sealed class SparePartRmaCase
{
    public int Id{get;set;} public string CaseNumber{get;set;}=""; public int QuarantineId{get;set;}
    public int InventoryItemId{get;set;} public int SupplierId{get;set;} public decimal Quantity{get;set;}
    public string AuthorizationNumber{get;set;}=""; public string Status{get;set;}=SparePartRmaStatus.Open;
    public string Courier{get;set;}=""; public string TrackingNumber{get;set;}=""; public string ShippedAt{get;set;}="";
    public string DueDate{get;set;}=""; public string Outcome{get;set;}=""; public decimal ShippingCost{get;set;}
    public decimal ResolutionCost{get;set;} public string Notes{get;set;}=""; public string CreatedAt{get;set;}=DateTime.Now.ToString("s");
    public string UpdatedAt{get;set;}=DateTime.Now.ToString("s"); public string ClosedAt{get;set;}="";
}
