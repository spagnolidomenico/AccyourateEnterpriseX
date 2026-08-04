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
    public string UpdatedAt{get;set;}=""; public string CompletedAt{get;set;}=""; public string CancelledAt{get;set;}=""; public string ResolutionNotes{get;set;}="";
    public string Priority{get;set;}="Normale"; public string Owner{get;set;}=""; public string SupplierName{get;set;}="";
    public string CommunicationStatus{get;set;}="Registrata"; public int ParentInteractionId{get;set;} public string SentAt{get;set;}=""; public string ResponseReceivedAt{get;set;}="";
}
public sealed class SupplierFollowUpAudit
{
    public int Id{get;set;} public int InteractionId{get;set;} public string Action{get;set;}=""; public string OldValue{get;set;}="";
    public string NewValue{get;set;}=""; public string Notes{get;set;}=""; public string CreatedAt{get;set;}=""; public string CreatedBy{get;set;}="";
}
public sealed class SupplierFollowUpAutomationRule
{
    public int SupplierId{get;set;} public string SupplierName{get;set;}=""; public int ReminderDaysBefore{get;set;}=2;
    public int EscalateAfterDays{get;set;}=1; public bool IsEnabled{get;set;}=true; public string UpdatedAt{get;set;}="";
}
public sealed class SupplierFollowUpAutomationLog
{
    public int Id{get;set;} public int InteractionId{get;set;} public int SupplierId{get;set;} public string SupplierName{get;set;}="";
    public string EventType{get;set;}=""; public string Description{get;set;}=""; public string CreatedAt{get;set;}="";
}
public sealed class SupplierPortalAttachment
{
    public int Id{get;set;} public int SupplierId{get;set;} public int RmaId{get;set;} public string Category{get;set;}="Documento";
    public string FileName{get;set;}=""; public string StoredPath{get;set;}=""; public string Notes{get;set;}=""; public string CreatedAt{get;set;}="";
    public int InteractionId{get;set;} public bool IsAvailable=>File.Exists(StoredPath);
}
public sealed class SupplierEmailTemplate
{
    public int Id{get;set;} public string Name{get;set;}=""; public string SubjectTemplate{get;set;}="";
    public string BodyTemplate{get;set;}=""; public bool IsDefault{get;set;} public string UpdatedAt{get;set;}="";
}
