namespace Accyourate.App.AssetManagement.Models;

public sealed class SupplierPortalContact
{
    public int Id{get;set;} public int SupplierId{get;set;} public string Name{get;set;}=""; public string Role{get;set;}="";
    public string Email{get;set;}=""; public string Phone{get;set;}=""; public bool IsPrimary{get;set;} public string Notes{get;set;}=""; public string CreatedAt{get;set;}="";
}
public sealed class SupplierPortalInteraction
{
    public int Id{get;set;} public int SupplierId{get;set;} public int RmaId{get;set;} public string Type{get;set;}="Comunicazione";
    public string Direction{get;set;}="In uscita"; public string Subject{get;set;}=""; public string Message{get;set;}=""; public string Status{get;set;}="Aperta";
    public string FollowUpDate{get;set;}=""; public string CreatedAt{get;set;}=""; public string CreatedBy{get;set;}="";
}
public sealed class SupplierPortalAttachment
{
    public int Id{get;set;} public int SupplierId{get;set;} public int RmaId{get;set;} public string Category{get;set;}="Documento";
    public string FileName{get;set;}=""; public string StoredPath{get;set;}=""; public string Notes{get;set;}=""; public string CreatedAt{get;set;}="";
}
