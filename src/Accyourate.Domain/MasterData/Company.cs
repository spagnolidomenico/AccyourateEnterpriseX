namespace Accyourate.Domain.MasterData;

public sealed class Company
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string VatNumber { get; set; } = string.Empty;
    public string FiscalCode { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
