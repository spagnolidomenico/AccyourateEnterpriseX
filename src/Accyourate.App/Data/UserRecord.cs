namespace Accyourate.App.Data;

public sealed class UserRecord
{
    public long Id { get; set; }
    public string Username { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = "Admin";
    public bool IsActive { get; set; } = true;
    public string CreatedAt { get; set; } = "";
}
