using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.Api;
using Accyourate.App.Application.Services;
using Accyourate.App.Data;
using Accyourate.App.Infrastructure.Configuration;
using Accyourate.App.Infrastructure.Database;

namespace Accyourate.App;

public sealed class ArchitectureWindow : Window
{
    private readonly DatabaseService _database;
    private readonly AppConfiguration _configuration;
    private readonly StackPanel _content = new();

    public ArchitectureWindow(DatabaseService database)
    {
        _database = database;
        _configuration = new AppConfigurationService().Load();

        Title = "Accyourate Enterprise X - Enterprise Architecture";
        Width = 1120;
        Height = 760;
        MinWidth = 1024;
        MinHeight = 680;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = Brush.Parse("#F7F7F6");

        Content = BuildLayout();
        Refresh();
    }

    private Control BuildLayout()
    {
        var stack = new StackPanel { Margin = new Avalonia.Thickness(24), Spacing = 16 };

        stack.Children.Add(new TextBlock
        {
            Text = "Enterprise Architecture",
            FontSize = 28,
            FontWeight = FontWeight.Bold,
            Foreground = Brush.Parse("#B5162B")
        });

        stack.Children.Add(new TextBlock
        {
            Text = "Versione 5.6: configurazione, logging, migrazioni, servizi applicativi e API foundation.",
            TextWrapping = TextWrapping.Wrap
        });

        stack.Children.Add(Card(_content));

        return new ScrollViewer
        {
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            Content = stack
        };
    }

    private void Refresh()
    {
        try
        {
            _content.Children.Clear();
            _content.Spacing = 14;

            _content.Children.Add(Section("Health Report"));
            var health = new ApplicationHealthService(_database, _configuration);
            foreach (var line in health.GetHealthReport())
                _content.Children.Add(new TextBlock { Text = line });

            _content.Children.Add(new Separator());

            _content.Children.Add(Section("Piano migrazioni database"));
            foreach (var migration in DatabaseMigrationPlan.Migrations)
                _content.Children.Add(new TextBlock { Text = $"{migration.Version} - {migration.Name} ({migration.ScriptFile})" });

            _content.Children.Add(new Separator());

            _content.Children.Add(Section("API foundation"));
            foreach (var endpoint in ApiFoundationRegistry.PlannedEndpoints)
                _content.Children.Add(new TextBlock { Text = $"{endpoint.Method} {endpoint.Route} - {endpoint.Description} [{endpoint.Status}]" });

            _content.Children.Add(new Separator());

            _content.Children.Add(Section("Feature flags"));
            _content.Children.Add(new TextBlock { Text = $"API Foundation: {_configuration.ApiFoundationEnabled}" });
            _content.Children.Add(new TextBlock { Text = $"Plugin System: {_configuration.PluginSystemEnabled}" });
            _content.Children.Add(new TextBlock { Text = $"Auto Update: {_configuration.AutoUpdateEnabled}" });
            _content.Children.Add(new TextBlock { Text = $"Telemetry: {_configuration.TelemetryEnabled}" });
        }
        catch (Exception ex)
        {
            _content.Children.Clear();
            _content.Children.Add(Section("Errore Enterprise Architecture"));
            _content.Children.Add(new TextBlock
            {
                Text = ex.Message,
                TextWrapping = TextWrapping.Wrap
            });
        }
    }

    private static TextBlock Section(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 18,
            FontWeight = FontWeight.Bold,
            Foreground = Brush.Parse("#B5162B")
        };
    }

    private static Border Card(Control content)
    {
        return new Border
        {
            Background = Brushes.White,
            CornerRadius = new Avalonia.CornerRadius(14),
            Padding = new Avalonia.Thickness(18),
            Child = content
        };
    }
}
