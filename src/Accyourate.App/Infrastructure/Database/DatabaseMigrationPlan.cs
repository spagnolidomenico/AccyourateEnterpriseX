namespace Accyourate.App.Infrastructure.Database;

public static class DatabaseMigrationPlan
{
    public static IReadOnlyList<DatabaseMigrationRecord> Migrations { get; } = new List<DatabaseMigrationRecord>
    {
        new() { Version = "1.0", Name = "Core security and audit", ScriptFile = "001_core.sql" },
        new() { Version = "2.0", Name = "People and Asset IT", ScriptFile = "002_people_assets.sql" },
        new() { Version = "3.0", Name = "Workflow foundation", ScriptFile = "003_workflow.sql" },
        new() { Version = "4.0", Name = "Medical Device Foundation", ScriptFile = "004_medical.sql" },
        new() { Version = "4.1", Name = "Production and Quality", ScriptFile = "004_001_production_quality.sql" },
        new() { Version = "4.2", Name = "Warehouse and Logistics", ScriptFile = "004_002_warehouse_logistics.sql" },
        new() { Version = "4.3", Name = "Laundry and Maintenance", ScriptFile = "004_003_laundry_maintenance.sql" },
        new() { Version = "5.0", Name = "Document Management", ScriptFile = "005_documents.sql" },
        new() { Version = "5.6", Name = "Enterprise Architecture Foundation", ScriptFile = "005_006_architecture.sql" }
    };
}
