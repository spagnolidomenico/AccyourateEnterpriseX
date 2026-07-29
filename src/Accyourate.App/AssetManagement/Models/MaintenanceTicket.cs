namespace Accyourate.App.AssetManagement.Models;

public sealed class MaintenanceTicket
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = "Media";
    public string Status { get; set; } = "Aperto";
    public string OpenedAt { get; set; } = DateTime.Now.ToString("s");
    public string ClosedAt { get; set; } = string.Empty;
    public string Technician { get; set; } = string.Empty;
    public string ResolutionNotes { get; set; } = string.Empty;
    public string ScheduledAt { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public string PdfPath { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = DateTime.Now.ToString("s");
    public string OverdueNotifiedAt { get; set; } = string.Empty;
    public int ReminderDays { get; set; } = 7;
    public int RecurrenceMonths { get; set; }
    public int NextTicketId { get; set; }
    public string ReminderNotifiedAt { get; set; } = string.Empty;
    public string WorkStartedAt { get; set; } = string.Empty;
    public string SlaDeadline { get; set; } = string.Empty;
    public int DowntimeMinutes { get; set; }
    public string SlaBreachedNotifiedAt { get; set; } = string.Empty;
}
