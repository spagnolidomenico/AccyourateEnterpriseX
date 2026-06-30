namespace Accyourate.App.Infrastructure.Configuration;

public sealed class AppConfiguration
{
    public string ApplicationName { get; set; } = "Accyourate Enterprise X";
    public string Version { get; set; } = "5.6";
    public string Environment { get; set; } = "Development";
    public bool LoggingEnabled { get; set; } = true;
    public string LoggingLevel { get; set; } = "Information";
    public bool ApiFoundationEnabled { get; set; } = true;
    public bool PluginSystemEnabled { get; set; } = false;
    public bool AutoUpdateEnabled { get; set; } = false;
    public bool TelemetryEnabled { get; set; } = false;
}
