namespace Accyourate.App.AssetManagement.DeliveryReports;

public sealed class DeliveryReport
{
    public int Id { get; set; }
    public string ReportNumber { get; set; } = string.Empty;
    public int AssignmentId { get; set; }
    public int AssetId { get; set; }
    public int AssetEmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string AssetCode { get; set; } = string.Empty;
    public string ReportDate { get; set; } = DateTime.Now.ToString("s");
    public string Status { get; set; } = DeliveryReportStatus.Draft;
    public string PdfPath { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = "System";
    public string CreatedAt { get; set; } = DateTime.Now.ToString("s");
    public string UpdatedAt { get; set; } = DateTime.Now.ToString("s");
}
