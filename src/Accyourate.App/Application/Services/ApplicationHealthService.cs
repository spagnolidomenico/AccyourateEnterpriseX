using Accyourate.App.Application.Contracts;
using Accyourate.App.Data;
using Accyourate.App.Infrastructure.Configuration;

namespace Accyourate.App.Application.Services;

public sealed class ApplicationHealthService : IApplicationService
{
    private readonly DatabaseService _database;
    private readonly AppConfiguration _configuration;

    public string ServiceName => "Application Health";

    public ApplicationHealthService(DatabaseService database, AppConfiguration configuration)
    {
        _database = database;
        _configuration = configuration;
    }

    public List<string> GetHealthReport()
    {
        return new List<string>
        {
            $"Applicazione: {_configuration.ApplicationName}",
            $"Versione: {_configuration.Version}",
            $"Ambiente: {_configuration.Environment}",
            $"Database: {AppPaths.DatabasePath}",
            $"Persone: {_database.CountTable("employees")}",
            $"Asset IT: {_database.CountTable("assets")}",
            $"Dispositivi Medici: {_database.CountTable("medical_devices")}",
            $"Documenti: {_database.CountTable("documents")}",
            $"Workflow events: {_database.CountTable("workflow_events")}",
            $"Audit records: {_database.CountTable("audit_logs")}"
        };
    }
}
