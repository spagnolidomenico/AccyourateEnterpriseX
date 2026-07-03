using Accyourate.App.AssetManagement.Services;
using Accyourate.App.Platform.Audit;
using Accyourate.App.Platform.Notifications;

namespace Accyourate.App.AssetManagement.DeliveryReports;

public sealed class DeliveryReportService
{
    private readonly DeliveryReportRepository _repository;
    private readonly AssetAssignmentEngine _assignmentEngine;
    private readonly AuditService _audit;
    private readonly NotificationService _notifications;

    public DeliveryReportService(
        DeliveryReportRepository? repository = null,
        AssetAssignmentEngine? assignmentEngine = null,
        AuditService? audit = null,
        NotificationService? notifications = null)
    {
        _repository = repository ?? new DeliveryReportRepository();
        _assignmentEngine = assignmentEngine ?? new AssetAssignmentEngine();
        _audit = audit ?? new AuditService();
        _notifications = notifications ?? new NotificationService();
    }

    public int CreateFromActiveAsset(int assetId, string createdBy = "System", string notes = "")
    {
        var assignment = _assignmentEngine.GetActiveAssignmentForAsset(assetId)
            ?? throw new InvalidOperationException("Nessuna assegnazione attiva trovata per l'asset.");

        return CreateFromAssignment(assignment, createdBy, notes);
    }

    public int CreateFromAssignment(AssetAssignmentSummary assignment, string createdBy = "System", string notes = "")
    {
        var report = new DeliveryReport
        {
            AssignmentId = assignment.AssignmentId,
            AssetId = assignment.AssetId,
            AssetEmployeeId = assignment.AssetEmployeeId,
            EmployeeName = assignment.EmployeeName,
            AssetCode = assignment.AssetCode,
            ReportDate = DateTime.Now.ToString("s"),
            Status = DeliveryReportStatus.Draft,
            Notes = notes,
            CreatedBy = createdBy
        };

        var description = $"{assignment.Manufacturer} {assignment.Model}".Trim();

        var items = new List<DeliveryReportItem>
        {
            new()
            {
                AssetId = assignment.AssetId,
                AssetCode = assignment.AssetCode,
                Description = string.IsNullOrWhiteSpace(description) ? assignment.AssetCode : description,
                Notes = assignment.Notes
            }
        };

        var id = _repository.Create(report, items);

        _audit.Track(
            AuditAction.Created,
            $"Creato verbale consegna {report.ReportNumber} per {assignment.EmployeeName}",
            "DeliveryReport",
            id.ToString(),
            report.ReportNumber,
            createdBy,
            AuditSeverity.Info,
            "AssetManagement");

        _notifications.Publish(
            "Verbale consegna creato",
            $"Creato verbale {report.ReportNumber} per {assignment.EmployeeName}.",
            NotificationCategory.Asset,
            NotificationPriority.Info,
            createdBy,
            "open-delivery-report",
            id.ToString());

        return id;
    }

    public string GeneratePdf(int deliveryReportId, string generatedBy = "System") => new DeliveryReportPdfService(_repository).GeneratePdf(deliveryReportId, generatedBy);

    public IReadOnlyList<DeliveryReport> GetLatest(int limit = 50) => _repository.GetLatest(limit);
    public IReadOnlyList<DeliveryReport> GetByAssetId(int assetId) => _repository.GetByAssetId(assetId);
    public IReadOnlyList<DeliveryReport> GetByEmployeeName(string employeeName) => _repository.GetByEmployeeName(employeeName);
    public IReadOnlyList<DeliveryReportItem> GetItems(int deliveryReportId) => _repository.GetItems(deliveryReportId);
}
