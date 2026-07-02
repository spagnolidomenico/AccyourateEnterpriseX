using Microsoft.Data.Sqlite;

namespace Accyourate.Infrastructure.Data;

public abstract class SqliteRepositoryBase
{
    protected SqliteRepositoryBase(AccyourateDatabaseContext context)
    {
        Context = context;
    }

    protected AccyourateDatabaseContext Context { get; }

    protected SqliteConnection CreateConnection()
    {
        return Context.CreateConnection();
    }

    protected static string ReadString(SqliteDataReader reader, int index)
    {
        return reader.IsDBNull(index) ? string.Empty : reader.GetString(index);
    }
}
