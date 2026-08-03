using Accyourate.App.AssetManagement.Models;

namespace Accyourate.App.AssetManagement.Services;

public sealed class SparePartRmaAnalyticsService
{
    private readonly SparePartRmaRepository _rma;
    private readonly MaintenancePurchasingRepository _purchasing;
    public SparePartRmaAnalyticsService(SparePartRmaRepository? rma=null,MaintenancePurchasingRepository? purchasing=null){_rma=rma??new SparePartRmaRepository();_purchasing=purchasing??new MaintenancePurchasingRepository();}

    public IReadOnlyList<SupplierRmaPerformance> GetPerformance()
    {
        var suppliers=_purchasing.GetSuppliers().ToDictionary(x=>x.Id);
        return _rma.GetAll().Where(x=>x.SupplierId>0).GroupBy(x=>x.SupplierId).Select(group=>Build(group,suppliers.TryGetValue(group.Key,out var supplier)?supplier.Name:$"Fornitore #{group.Key}")).OrderByDescending(x=>x.TotalCases).ThenBy(x=>x.SupplierName).ToList();
    }

    private static SupplierRmaPerformance Build(IGrouping<int,SparePartRmaCase> group,string name)
    {
        var cases=group.ToList();var closed=cases.Where(x=>x.Status==SparePartRmaStatus.Closed).ToList();
        var evaluated=closed.Where(x=>DateTime.TryParse(x.DueDate,out _)&&DateTime.TryParse(x.ClosedAt,out _)).ToList();
        var resolutionDays=closed.Select(x=>Days(x.CreatedAt,x.ClosedAt)).Where(x=>x.HasValue).Select(x=>x!.Value).ToList();
        return new SupplierRmaPerformance
        {
            SupplierId=group.Key,SupplierName=name,TotalCases=cases.Count,ActiveCases=cases.Count(IsActive),ClosedCases=closed.Count,
            OverdueCases=cases.Count(IsOverdue),EvaluatedSlaCases=evaluated.Count,OnTimeCases=evaluated.Count(IsOnTime),
            RepairCases=cases.Count(x=>x.Outcome==SparePartRmaOutcome.Repair),ReplacementCases=cases.Count(x=>x.Outcome==SparePartRmaOutcome.Replacement),RefundCases=cases.Count(x=>x.Outcome==SparePartRmaOutcome.Refund),
            TotalCost=cases.Sum(x=>x.ShippingCost+x.ResolutionCost),AverageResolutionDays=resolutionDays.Count==0?0:resolutionDays.Average()
        };
    }
    private static bool IsActive(SparePartRmaCase x)=>x.Status is not (SparePartRmaStatus.Closed or SparePartRmaStatus.Cancelled);
    private static bool IsOverdue(SparePartRmaCase x)=>IsActive(x)&&DateTime.TryParse(x.DueDate,out var due)&&due.Date<DateTime.Today;
    private static bool IsOnTime(SparePartRmaCase x)=>DateTime.TryParse(x.DueDate,out var due)&&DateTime.TryParse(x.ClosedAt,out var closed)&&closed.Date<=due.Date;
    private static double? Days(string start,string end)=>DateTime.TryParse(start,out var a)&&DateTime.TryParse(end,out var b)?Math.Max(0,(b-a).TotalDays):null;
}
