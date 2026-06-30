using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.Data;

namespace Accyourate.App;

public sealed class DiagnosticsWindow : Window
{
    private readonly DatabaseService _database;

    public DiagnosticsWindow(DatabaseService database)
    {
        _database = database;

        Title = "Accyourate Enterprise X - Diagnostica";
        Width = 760;
        Height = 560;
        
        MinWidth = 1024;
        MinHeight = 680;
WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = Brush.Parse("#F7F7F6");
        Content = BuildLayout();
    }

    private Control BuildLayout()
    {
        var stack = new StackPanel { Margin = new Thickness(24), Spacing = 14 };

        stack.Children.Add(new TextBlock
        {
            Text = "Diagnostica Database",
            FontSize = 26,
            FontWeight = FontWeight.Bold,
            Foreground = Brush.Parse("#B5162B")
        });

        var d = _database.GetDiagnostics();

        stack.Children.Add(MakeCard("Percorso database", d.DatabasePath));
        stack.Children.Add(MakeCard("File esistente", d.Exists ? "Sì" : "No"));
        stack.Children.Add(MakeCard("Dimensione", $"{d.SizeBytes} byte"));
        stack.Children.Add(MakeCard("Utenti totali", d.UsersCount.ToString()));
        stack.Children.Add(MakeCard("Utenti attivi", d.ActiveUsersCount.ToString()));
        stack.Children.Add(MakeCard("Eventi audit", d.AuditCount.ToString()));

        var auditTitle = new TextBlock
        {
            Text = "Ultimi eventi audit",
            FontSize = 18,
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(0, 18, 0, 0)
        };
        stack.Children.Add(auditTitle);

        foreach (var row in _database.GetRecentAudit(10))
        {
            stack.Children.Add(new TextBlock
            {
                Text = $"{row.CreatedAt} - {row.Username} - {row.Action} - {row.Details}",
                FontSize = 12
            });
        }

        return new ScrollViewer { Content = stack };
    }

    private static Control MakeCard(string label, string value)
    {
        return new Border
        {
            Background = Brushes.White,
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(14),
            Child = new StackPanel
            {
                Children =
                {
                    new TextBlock { Text = label, FontWeight = FontWeight.Bold },
                    new TextBlock { Text = value }
                }
            }
        };
    }
}
