namespace Accyourate.App.Platform.Settings;

public sealed class CompanySettings
{
    public string CompanyName { get; set; } = "Accyourate Group";
    public string LegalName { get; set; } = "Accyourate Group";
    public string VatNumber { get; set; } = string.Empty;
    public string FiscalCode { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string Country { get; set; } = "Italia";
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Pec { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string LogoPath { get; set; } = string.Empty;
}
