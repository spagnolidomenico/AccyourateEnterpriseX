using Accyourate.App.AssetManagement.Models;
using Accyourate.App.Platform.Pdf;

namespace Accyourate.App.AssetManagement.Services;

public sealed class SupplierRmaReportPdfService
{
    public string Generate(MaintenanceSupplier supplier,SupplierRmaPerformance performance,IReadOnlyList<SparePartRmaCase> cases)
    {
        var document=new SimplePdfDocument{Title="Valutazione fornitore RMA"};document.Branding.DocumentLabel="REPORT FORNITORE RMA";document.Branding.DocumentCode=$"SUP-RMA-{supplier.Id:D5}";
        document.AddTitle("Valutazione prestazioni fornitore");
        document.AddHeading("Fornitore");document.AddKeyValue("Ragione sociale",supplier.Name);document.AddKeyValue("Partita IVA",Dash(supplier.VatNumber));document.AddKeyValue("Referente",Dash(supplier.ContactPerson));document.AddKeyValue("Contatti",string.Join(" - ",new[]{supplier.Email,supplier.Phone}.Where(x=>!string.IsNullOrWhiteSpace(x))));
        document.AddHeading("Indicatori");document.AddKeyValue("Pratiche RMA",performance.TotalCases.ToString());document.AddKeyValue("Aperte / scadute",$"{performance.ActiveCases} / {performance.OverdueCases}");document.AddKeyValue("Rispetto SLA",performance.EvaluatedSlaCases==0?"Non ancora valutabile":$"{performance.SlaCompliancePercent:N0}%");document.AddKeyValue("Tempo medio risoluzione",performance.ClosedCases==0?"Non ancora valutabile":$"{performance.AverageResolutionDays:N1} giorni");document.AddKeyValue("Costi complessivi",$"EUR {performance.TotalCost:N2}");document.AddKeyValue("Affidabilità automatica",$"{performance.ReliabilityScore:N0}/100");
        document.AddHeading("Valutazione responsabile acquisti");document.AddKeyValue("Valutazione",performance.ManualRating<=0?"Non assegnata":$"{performance.ManualRating}/5");document.AddKeyValue("Punteggio combinato",$"{performance.CombinedScore:N0}/100");document.AddKeyValue("Note",Dash(performance.ManualNotes));
        document.AddHeading("Esiti");document.AddKeyValue("Riparazioni",performance.RepairCases.ToString());document.AddKeyValue("Sostituzioni",performance.ReplacementCases.ToString());document.AddKeyValue("Rimborsi",performance.RefundCases.ToString());
        document.AddHeading("Storico pratiche");
        if(cases.Count==0)document.AddKeyValue("Pratiche","Nessuna pratica disponibile");
        foreach(var item in cases.Take(30))document.AddKeyValue(item.CaseNumber,CaseSummary(item));
        document.AddSignaturePair("Responsabile acquisti","Responsabile qualità");
        var folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"Accyourate Enterprise X","Report Fornitori RMA");return new PdfExportService().Export(document,folder,$"Fornitore-RMA-{supplier.Id:D5}");
    }
    private static string CaseSummary(SparePartRmaCase item){var parts=new List<string>{Date(item.CreatedAt),item.Status};if(!string.IsNullOrWhiteSpace(item.Outcome))parts.Add(item.Outcome);parts.Add($"EUR {item.ShippingCost+item.ResolutionCost:N2}");return string.Join(" - ",parts);}
    private static string Dash(string value)=>string.IsNullOrWhiteSpace(value)?"—":value;private static string Date(string value)=>DateTime.TryParse(value,out var date)?date.ToString("dd/MM/yyyy"):value;
}
