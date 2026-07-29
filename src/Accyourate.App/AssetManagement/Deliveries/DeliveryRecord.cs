namespace Accyourate.App.AssetManagement.Deliveries;

/// <summary>
/// Rappresenta l'assegnazione temporale di un asset a un dipendente.
/// Il modello è indipendente dalla UI e costituisce la base del Registro Consegne.
/// </summary>
public sealed class DeliveryRecord
{
    public int Id { get; set; }

    public int AssetId { get; set; }

    public int EmployeeId { get; set; }

    public string DeliveryDate { get; set; } = DateTime.Now.ToString("s");

    public string ReturnDate { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public string ReturnCondition { get; set; } = string.Empty;

    public string ReturnNotes { get; set; } = string.Empty;

    public string ReturnPdfPath { get; set; } = string.Empty;

    public string Status { get; set; } = DeliveryRecordStatus.Active;

    public string CreatedAt { get; set; } = DateTime.Now.ToString("s");

    public string UpdatedAt { get; set; } = DateTime.Now.ToString("s");

    /// <summary>
    /// Indica se la consegna risulta ancora attiva.
    /// </summary>
    public bool IsActive => Status == DeliveryRecordStatus.Active && string.IsNullOrWhiteSpace(ReturnDate);
}
