using Accyourate.App.Data;
using Accyourate.App.Models;

namespace Accyourate.App.ActionEngine;

public sealed class ActionContext
{
    public DatabaseService Database { get; }
    public CurrentUser User { get; }

    public ActionContext(DatabaseService database, CurrentUser user)
    {
        Database = database;
        User = user;
    }
}
