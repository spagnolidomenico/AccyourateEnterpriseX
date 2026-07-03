using Microsoft.Data.Sqlite;
using Accyourate.App.HumanResources.Database;

namespace Accyourate.App.HumanResources.Repositories;

public abstract class HrRepositoryBase
{
    private readonly HrDatabase _database;

    protected HrRepositoryBase(HrDatabase? database = null)
    {
        _database = database ?? new HrDatabase();
    }

    protected SqliteConnection OpenConnection()
    {
        return _database.OpenConnection();
    }

    protected static string ReadString(SqliteDataReader reader, int index)
    {
        return reader.IsDBNull(index) ? string.Empty : reader.GetString(index);
    }

    protected static int? ReadNullableInt(SqliteDataReader reader, int index)
    {
        return reader.IsDBNull(index) ? null : reader.GetInt32(index);
    }
}
