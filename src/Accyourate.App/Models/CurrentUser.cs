using System.Collections.Generic;

namespace Accyourate.App.Models;

public sealed class CurrentUser
{
    public string Username { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public HashSet<string> Permissions { get; init; } = new();

    public bool IsAdmin => Role == "Admin" || Permissions.Contains("*");

    public bool Can(string permission)
    {
        return IsAdmin || Permissions.Contains(permission);
    }
}
