namespace Accyourate.App.AssetManagement.Models;

public sealed class SupplierRmaPerformance
{
    public int SupplierId{get;set;}
    public string SupplierName{get;set;}="";
    public int TotalCases{get;set;}
    public int ActiveCases{get;set;}
    public int ClosedCases{get;set;}
    public int OverdueCases{get;set;}
    public int OnTimeCases{get;set;}
    public int EvaluatedSlaCases{get;set;}
    public int RepairCases{get;set;}
    public int ReplacementCases{get;set;}
    public int RefundCases{get;set;}
    public decimal TotalCost{get;set;}
    public double AverageResolutionDays{get;set;}
    public int ManualRating{get;set;}
    public string ManualNotes{get;set;}="";
    public string EvaluationUpdatedAt{get;set;}="";
    public double SlaCompliancePercent=>EvaluatedSlaCases==0?0:OnTimeCases*100d/EvaluatedSlaCases;
    public double ReliabilityScore=>Math.Clamp(SlaCompliancePercent*0.7+(ClosedCases==0?0:Math.Min(100,ClosedCases*100d/Math.Max(1,TotalCases)))*0.3,0,100);
    public double CombinedScore=>ManualRating<=0?ReliabilityScore:ReliabilityScore*0.75+(ManualRating*20d)*0.25;
}
