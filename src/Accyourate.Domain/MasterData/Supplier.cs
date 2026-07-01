namespace Accyourate.Domain.MasterData;

public sealed class Supplier
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string VatNumber { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
