using Accyourate.App.Data;
using Accyourate.App.Models;

namespace Accyourate.App.Security;

public sealed class AuthenticationService
{
    private readonly DatabaseService _database;

    public AuthenticationService(DatabaseService database)
    {
        _database = database;
    }

    public CurrentUser? Login(string username, string password)
    {
        var user = _database.FindActiveUser(username);
        if (user is null)
            return null;

        if (!PasswordHasher.Verify(password, user.PasswordHash))
            return null;

        _database.WriteAudit(user.Username, "LOGIN_SUCCESS", "Accesso riuscito");

        return new CurrentUser
        {
            Username = user.Username,
            DisplayName = user.DisplayName,
            Role = user.Role,
            Permissions = _database.GetPermissionsForRole(user.Role)
        };
    }
}
