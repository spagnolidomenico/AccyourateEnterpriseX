using Microsoft.Data.Sqlite;
using Accyourate.Core.Repositories;
using Accyourate.Domain.Assets;
using Accyourate.Infrastructure.Data;

namespace Accyourate.Infrastructure.Repositories;

public sealed class AssetDatabaseAssetRepository : SqliteRepositoryBase, IAssetRepository
{
    public AssetDatabaseAssetRepository()
        : this(AccyourateDatabaseContextFactory.CreateAssetManagementContext())
    {
    }

    public AssetDatabaseAssetRepository(AccyourateDatabaseContext context) : base(context)
    {
    }

    public IReadOnlyList<Asset> GetAll()
    {
        using var connection = CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, AssetCode, Category, Manufacturer, Model, SerialNumber, AssetTag, Status,
                   PurchaseDate, WarrantyEndDate, OperatingSystem, BitLockerEnabled, Notes
            FROM Assets
            ORDER BY AssetCode;
        """;

        using var reader = command.ExecuteReader();
        var result = new List<Asset>();

        while (reader.Read())
            result.Add(ReadAsset(reader));

        return result;
    }

    public Asset? GetById(int id)
    {
        using var connection = CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, AssetCode, Category, Manufacturer, Model, SerialNumber, AssetTag, Status,
                   PurchaseDate, WarrantyEndDate, OperatingSystem, BitLockerEnabled, Notes
            FROM Assets
            WHERE Id = $id;
        """;
        command.Parameters.AddWithValue("$id", id);

        using var reader = command.ExecuteReader();
        return reader.Read() ? ReadAsset(reader) : null;
    }

    public IReadOnlyList<Asset> Search(string query)
    {
        using var connection = CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, AssetCode, Category, Manufacturer, Model, SerialNumber, AssetTag, Status,
                   PurchaseDate, WarrantyEndDate, OperatingSystem, BitLockerEnabled, Notes
            FROM Assets
            WHERE AssetCode LIKE $query
               OR Category LIKE $query
               OR Manufacturer LIKE $query
               OR Model LIKE $query
               OR SerialNumber LIKE $query
               OR Status LIKE $query
            ORDER BY AssetCode;
        """;
        command.Parameters.AddWithValue("$query", $"%{query}%");

        using var reader = command.ExecuteReader();
        var result = new List<Asset>();

        while (reader.Read())
            result.Add(ReadAsset(reader));

        return result;
    }

    public IReadOnlyList<Asset> GetByStatus(string status)
    {
        using var connection = CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, AssetCode, Category, Manufacturer, Model, SerialNumber, AssetTag, Status,
                   PurchaseDate, WarrantyEndDate, OperatingSystem, BitLockerEnabled, Notes
            FROM Assets
            WHERE Status = $status
            ORDER BY AssetCode;
        """;
        command.Parameters.AddWithValue("$status", status);

        using var reader = command.ExecuteReader();
        var result = new List<Asset>();

        while (reader.Read())
            result.Add(ReadAsset(reader));

        return result;
    }

    public IReadOnlyList<Asset> GetAvailableForAssignment()
    {
        using var connection = CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, AssetCode, Category, Manufacturer, Model, SerialNumber, AssetTag, Status,
                   PurchaseDate, WarrantyEndDate, OperatingSystem, BitLockerEnabled, Notes
            FROM Assets
            WHERE Status <> 'Assegnato'
            ORDER BY AssetCode;
        """;

        using var reader = command.ExecuteReader();
        var result = new List<Asset>();

        while (reader.Read())
            result.Add(ReadAsset(reader));

        return result;
    }

    public int Create(Asset asset)
    {
        using var connection = CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Assets (
                AssetCode, Category, Manufacturer, Model, SerialNumber, AssetTag, Status,
                PurchaseDate, WarrantyEndDate, OperatingSystem, BitLockerEnabled, Notes, CreatedAt, UpdatedAt
            )
            VALUES (
                $AssetCode, $Category, $Manufacturer, $Model, $SerialNumber, $AssetTag, $Status,
                $PurchaseDate, $WarrantyEndDate, $OperatingSystem, $BitLockerEnabled, $Notes, $Now, $Now
            );
            SELECT last_insert_rowid();
        """;
        AddParameters(command, asset);
        command.Parameters.AddWithValue("$Now", DateTime.Now.ToString("s"));

        return Convert.ToInt32(command.ExecuteScalar());
    }

    public void Update(Asset asset)
    {
        using var connection = CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE Assets
            SET AssetCode = $AssetCode,
                Category = $Category,
                Manufacturer = $Manufacturer,
                Model = $Model,
                SerialNumber = $SerialNumber,
                AssetTag = $AssetTag,
                Status = $Status,
                PurchaseDate = $PurchaseDate,
                WarrantyEndDate = $WarrantyEndDate,
                OperatingSystem = $OperatingSystem,
                BitLockerEnabled = $BitLockerEnabled,
                Notes = $Notes,
                UpdatedAt = $UpdatedAt
            WHERE Id = $Id;
        """;
        command.Parameters.AddWithValue("$Id", asset.Id);
        AddParameters(command, asset);
        command.Parameters.AddWithValue("$UpdatedAt", DateTime.Now.ToString("s"));
        command.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var connection = CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Assets WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
    }

    private static Asset ReadAsset(SqliteDataReader reader)
    {
        return new Asset
        {
            Id = reader.GetInt32(0),
            AssetCode = reader.GetString(1),
            Category = ReadString(reader, 2),
            Manufacturer = ReadString(reader, 3),
            Model = ReadString(reader, 4),
            SerialNumber = ReadString(reader, 5),
            AssetTag = ReadString(reader, 6),
            Status = ReadString(reader, 7),
            PurchaseDate = ReadString(reader, 8),
            WarrantyEndDate = ReadString(reader, 9),
            OperatingSystem = ReadString(reader, 10),
            BitLockerEnabled = reader.GetInt32(11) == 1,
            Notes = ReadString(reader, 12)
        };
    }

    private static void AddParameters(SqliteCommand command, Asset asset)
    {
        command.Parameters.AddWithValue("$AssetCode", asset.AssetCode);
        command.Parameters.AddWithValue("$Category", asset.Category);
        command.Parameters.AddWithValue("$Manufacturer", asset.Manufacturer);
        command.Parameters.AddWithValue("$Model", asset.Model);
        command.Parameters.AddWithValue("$SerialNumber", asset.SerialNumber);
        command.Parameters.AddWithValue("$AssetTag", asset.AssetTag);
        command.Parameters.AddWithValue("$Status", asset.Status);
        command.Parameters.AddWithValue("$PurchaseDate", asset.PurchaseDate);
        command.Parameters.AddWithValue("$WarrantyEndDate", asset.WarrantyEndDate);
        command.Parameters.AddWithValue("$OperatingSystem", asset.OperatingSystem);
        command.Parameters.AddWithValue("$BitLockerEnabled", asset.BitLockerEnabled ? 1 : 0);
        command.Parameters.AddWithValue("$Notes", asset.Notes);
    }
}
