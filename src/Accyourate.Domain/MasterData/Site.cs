namespace Accyourate.Domain.MasterData;

public sealed class Site
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public bool IsMainSite { get; set; }
    public string Notes { get; set; } = string.Empty;
}
