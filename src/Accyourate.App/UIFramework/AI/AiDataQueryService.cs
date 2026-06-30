using Accyourate.App.Data;

namespace Accyourate.App.UIFramework.AI;

public sealed class AiDataQueryService
{
    private readonly DatabaseService _database;

    public AiDataQueryService(DatabaseService database)
    {
        _database = database;
    }

    public AiDataQueryResult CountEntity(string entity)
    {
        var table = entity switch
        {
            "medical" => "medical_devices",
            "documents" => "documents",
            "assets" => "assets",
            "people" => "employees",
            "quality" => "quality_tests",
            "maintenance" => "maintenance_records",
            _ => ""
        };

        if (string.IsNullOrWhiteSpace(table))
        {
            return new AiDataQueryResult
            {
                Entity = entity,
                Count = 0,
                Summary = "Entità non ancora supportata dal servizio dati AI."
            };
        }

        var count = SafeCount(table);

        return new AiDataQueryResult
        {
            Entity = entity,
            Count = count,
            Summary = $"Totale rilevato per {entity}: {count}."
        };
    }

    private int SafeCount(string table)
    {
        try
        {
            return _database.CountTable(table);
        }
        catch
        {
            return 0;
        }
    }
}
