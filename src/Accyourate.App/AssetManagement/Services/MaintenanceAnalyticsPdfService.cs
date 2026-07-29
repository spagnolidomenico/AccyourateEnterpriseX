using Accyourate.App.AssetManagement.Models;
using Accyourate.App.Platform.Pdf;
using Accyourate.App.Platform.Settings;

namespace Accyourate.App.AssetManagement.Services;

public sealed class MaintenanceAnalyticsPdfService
{
    private readonly SettingsService _settings = new();

    public string Generate(
        IReadOnlyList<MaintenanceTicket> tickets,
        IReadOnlyDictionary<int, Asset> assets,
        string generatedBy)
    {
        var settings = _settings.Load();
        var template = settings.DocumentTemplate ?? new DocumentTemplateSettings();
        var number = $"MRA-{DateTime.Now:yyyyMMdd-HHmmss}";
        var partTotals = new MaintenancePartsRepository().GetTotalsByTicket();
        var document = BuildDocument(tickets, assets, generatedBy, number, settings, template, partTotals);
        var folder = Path.Combine(
            Path.GetDirectoryName(_settings.GetDeliveryReportsFolder())!,
            "Report Manutenzioni");
        return new PdfExportService().Export(document, folder, number);
    }

    internal static SimplePdfDocument BuildDocument(
        IReadOnlyList<MaintenanceTicket> tickets,
        IReadOnlyDictionary<int, Asset> assets,
        string generatedBy,
        string number,
        ApplicationSettings settings,
        DocumentTemplateSettings template,
        IReadOnlyDictionary<int,decimal>? partTotals=null)
    {
        var document = new SimplePdfDocument { Title = $"Report manutenzioni {number}" };
        document.Branding.CompanyName = string.IsNullOrWhiteSpace(settings.Company.LegalName)
            ? settings.Company.CompanyName
            : settings.Company.LegalName;
        document.Branding.CompanyDetailLines.AddRange(new[]
        {
            settings.Company.Address,
            string.Join(" ", new[] { settings.Company.City, settings.Company.Province }
                .Where(value => !string.IsNullOrWhiteSpace(value))),
            settings.Company.Email
        }.Where(value => !string.IsNullOrWhiteSpace(value)));
        document.Branding.HeaderLayout = template.HeaderLayout;
        document.Branding.LogoPath = settings.Company.LogoPath;
        document.Branding.PrimaryColor = template.PrimaryColor;
        document.Branding.DocumentLabel = "REPORT ANALITICO MANUTENZIONI";
        document.Branding.DocumentCode = number;
        document.Branding.DocumentVersion = template.DocumentVersion;
        document.Branding.FooterText = template.FooterText;
        document.Branding.ConfidentialityText = template.ConfidentialityText;
        document.Branding.ShowLogo = template.ShowLogo;
        document.Branding.ShowCompanyDetails = template.ShowCompanyDetails;
        document.Branding.ShowDocumentMetadata = template.ShowDocumentMetadata;
        document.Branding.ShowFooter = template.ShowFooter;
        document.Branding.ShowPageNumber = template.ShowPageNumber;
        document.Branding.ShowPrintTimestamp = template.ShowPrintTimestamp;

        var completed = tickets.Where(ticket => ticket.Status == "Completato").ToList();
        var active = tickets.Where(ticket => ticket.Status != "Completato").ToList();
        decimal PartCost(int ticketId) =>
            partTotals is not null && partTotals.TryGetValue(ticketId,out var value) ? value : 0;
        var totalCost = completed.Sum(ticket => ticket.Cost + PartCost(ticket.Id));
        var averageHours = AverageResolutionHours(completed);
        var slaEligible = completed.Where(ticket =>
            DateTime.TryParse(ticket.SlaDeadline, out _) &&
            DateTime.TryParse(ticket.ClosedAt, out _)).ToList();
        var slaCompliant = slaEligible.Count == 0
            ? 0
            : 100d * slaEligible.Count(ticket =>
                DateTime.Parse(ticket.ClosedAt) <= DateTime.Parse(ticket.SlaDeadline)) / slaEligible.Count;
        var downtimeHours = completed.Sum(ticket => ticket.DowntimeMinutes) / 60d;

        document.AddHeading("Riepilogo");
        document.AddKeyValue("Generato da", generatedBy);
        document.AddKeyValue("Interventi totali", tickets.Count.ToString());
        document.AddKeyValue("Interventi attivi", active.Count.ToString());
        document.AddKeyValue("Interventi completati", completed.Count.ToString());
        document.AddKeyValue("Costo complessivo", $"EUR {totalCost:N2}");
        document.AddKeyValue("Tempo medio di risoluzione",
            averageHours > 0 ? $"{averageHours:N1} ore" : "Non disponibile");
        document.AddKeyValue("Conformità SLA",
            slaEligible.Count > 0 ? $"{slaCompliant:N0}%" : "Non disponibile");
        document.AddKeyValue("Tempo di fermo complessivo", $"{downtimeHours:N1} ore");

        document.AddHeading("Costi per asset");
        foreach (var group in completed
                     .GroupBy(ticket => ticket.AssetId)
                     .Select(group => new { AssetId = group.Key, Cost = group.Sum(item => item.Cost + PartCost(item.Id)), Count = group.Count() })
                     .OrderByDescending(item => item.Cost)
                     .Take(12))
        {
            assets.TryGetValue(group.AssetId, out var asset);
            document.AddKeyValue(
                asset?.AssetCode ?? $"Asset #{group.AssetId}",
                $"{group.Count} interventi - EUR {group.Cost:N2}");
        }

        document.AddHeading("Carico per tecnico");
        foreach (var group in tickets
                     .GroupBy(ticket => string.IsNullOrWhiteSpace(ticket.Technician) ? "Non assegnato" : ticket.Technician)
                     .OrderByDescending(group => group.Count())
                     .Take(12))
        {
            document.AddKeyValue(
                group.Key,
                $"{group.Count()} interventi - {group.Count(item => item.Status != "Completato")} attivi");
        }

        var partsCost = completed.Sum(ticket => PartCost(ticket.Id));
        document.AddHeading("Ricambi e fornitori");
        document.AddKeyValue("Costo complessivo ricambi", $"EUR {partsCost:N2}");
        document.AddKeyValue("Incidenza sui costi",
            totalCost > 0 ? $"{100m * partsCost / totalCost:N1}%" : "0%");

        document.AddHeading("Ricorrenze");
        var recurring = tickets.Where(ticket => ticket.RecurrenceMonths > 0).ToList();
        document.AddKeyValue("Interventi ricorrenti", recurring.Count.ToString());
        foreach (var ticket in recurring.Take(10))
        {
            assets.TryGetValue(ticket.AssetId, out var asset);
            document.AddKeyValue(
                asset?.AssetCode ?? $"Asset #{ticket.AssetId}",
                $"{ticket.Title} - ogni {ticket.RecurrenceMonths} mesi");
        }
        return document;
    }

    internal static double AverageResolutionHours(IEnumerable<MaintenanceTicket> tickets)
    {
        var durations = tickets
            .Select(ticket =>
                DateTime.TryParse(ticket.OpenedAt, out var opened) &&
                DateTime.TryParse(ticket.ClosedAt, out var closed) &&
                closed >= opened
                    ? (double?)(closed - opened).TotalHours
                    : null)
            .Where(value => value.HasValue)
            .Select(value => value!.Value)
            .ToList();
        return durations.Count == 0 ? 0 : durations.Average();
    }

}
