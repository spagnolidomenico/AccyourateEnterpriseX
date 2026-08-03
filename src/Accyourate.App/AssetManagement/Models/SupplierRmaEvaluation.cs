namespace Accyourate.App.AssetManagement.Models;

public sealed class SupplierRmaEvaluation
{
    public int SupplierId{get;set;}
    public int Rating{get;set;}
    public string Notes{get;set;}="";
    public string UpdatedBy{get;set;}="";
    public string UpdatedAt{get;set;}="";
}
