namespace Accyourate.App.AssetManagement.Deliveries;

/// <summary>
/// Stati supportati dal ciclo di vita di una consegna di attrezzatura.
/// I valori sono stringhe stabili per semplificare la futura persistenza su SQLite.
/// </summary>
public static class DeliveryRecordStatus
{
    public const string Planned = "Planned";
    public const string Active = "Active";
    public const string Returned = "Returned";
    public const string Cancelled = "Cancelled";

    public static bool IsValid(string? status)
    {
        return status is Planned or Active or Returned or Cancelled;
    }
}
