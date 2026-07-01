namespace Accyourate.App.AssetManagement.Models;

public sealed class AssetCredential
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public string CredentialType { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string SecretReference { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = DateTime.Now.ToString("s");
}
