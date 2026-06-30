using System.Text.Json;

namespace Accyourate.App.Infrastructure.Configuration;

public sealed class AppConfigurationService
{
    private readonly string _configurationPath;

    public AppConfigurationService()
    {
        _configurationPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
    }

    public AppConfiguration Load()
    {
        try
        {
            if (!File.Exists(_configurationPath))
                return new AppConfiguration();

            var json = File.ReadAllText(_configurationPath);
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            var config = new AppConfiguration();

            if (root.TryGetProperty("Application", out var app))
            {
                config.ApplicationName = app.TryGetProperty("Name", out var name) ? name.GetString() ?? config.ApplicationName : config.ApplicationName;
                config.Version = app.TryGetProperty("Version", out var version) ? version.GetString() ?? config.Version : config.Version;
                config.Environment = app.TryGetProperty("Environment", out var env) ? env.GetString() ?? config.Environment : config.Environment;
            }

            if (root.TryGetProperty("Logging", out var logging))
            {
                config.LoggingEnabled = logging.TryGetProperty("Enabled", out var enabled) ? enabled.GetBoolean() : config.LoggingEnabled;
                config.LoggingLevel = logging.TryGetProperty("Level", out var level) ? level.GetString() ?? config.LoggingLevel : config.LoggingLevel;
            }

            if (root.TryGetProperty("Features", out var features))
            {
                config.ApiFoundationEnabled = features.TryGetProperty("ApiFoundation", out var api) ? api.GetBoolean() : config.ApiFoundationEnabled;
                config.PluginSystemEnabled = features.TryGetProperty("PluginSystem", out var plugins) ? plugins.GetBoolean() : config.PluginSystemEnabled;
                config.AutoUpdateEnabled = features.TryGetProperty("AutoUpdate", out var updates) ? updates.GetBoolean() : config.AutoUpdateEnabled;
                config.TelemetryEnabled = features.TryGetProperty("Telemetry", out var telemetry) ? telemetry.GetBoolean() : config.TelemetryEnabled;
            }

            return config;
        }
        catch
        {
            return new AppConfiguration();
        }
    }
}
