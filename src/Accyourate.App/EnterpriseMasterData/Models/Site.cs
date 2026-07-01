namespace Accyourate.App.EnterpriseMasterData.Models;

public sealed class Site
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string Country { get; set; } = "Italia";
    public bool IsMainSite { get; set; }
    public string Notes { get; set; } = string.Empty;
}
