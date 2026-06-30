using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.Data;
using Accyourate.App.Infrastructure;
using Accyourate.App.Models;

namespace Accyourate.App;

public sealed class InfrastructureWindow : Window
{
    private readonly DatabaseService _database;
    private readonly CurrentUser _user;
    private readonly BackupService _backupService;
    private readonly StackPanel _content = new();
    private readonly TextBlock _message = new();

    public InfrastructureWindow(DatabaseService database, CurrentUser user)
    {
        _database = database;
        _user = user;
        _backupService = new BackupService(database);

        Title = "Accyourate Enterprise X - Project Infrastructure";
        Width = 960;
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
        var scroll = new ScrollViewer
        {
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        };
        var stack = new StackPanel { Margin = new Thickness(24), Spacing = 16 };

        stack.Children.Add(new TextBlock
        {
            Text = "Project Infrastructure",
            FontSize = 28,
            FontWeight = FontWeight.Bold,
            Foreground = Brush.Parse("#B5162B")
        });

        stack.Children.Add(new TextBlock
        {
            Text = "Developer 3.1: database versionato, configurazioni, backup e struttura release."
        });

        var backupButton = new Button
        {
            Content = "Crea Backup Database",
            Background = Brush.Parse("#B5162B"),
            Foreground = Brushes.White,
            FontWeight = FontWeight.Bold,
            Padding = new Thickness(12)
        };
        backupButton.Click += (_, _) => CreateBackup();
        stack.Children.Add(backupButton);

        _message.Foreground = Brush.Parse("#B5162B");
        stack.Children.Add(_message);

        stack.Children.Add(new Border
        {
            Background = Brushes.White,
            CornerRadius = new CornerRadius(14),
            Padding = new Thickness(18),
            Child = _content
        });

        scroll.Content = stack;
        return scroll;
    }

    private void CreateBackup()
    {
        var path = _backupService.CreateBackup(_user.Username);
        _message.Text = $"Backup creato: {path}";
        Refresh();
    }

    private void Refresh()
    {
        _content.Children.Clear();
        _content.Spacing = 18;

        _content.Children.Add(SectionTitle("Versioni Database"));
        foreach (var v in _database.GetDatabaseVersions())
        {
            _content.Children.Add(new TextBlock
            {
                Text = $"{v.Version} - {v.Description} - {v.AppliedAt}"
            });
        }

        _content.Children.Add(SectionTitle("Configurazioni"));
        foreach (var s in _database.GetSettings())
        {
            _content.Children.Add(new TextBlock
            {
                Text = $"[{s.GroupName}] {s.Key} = {s.Value}"
            });
        }

        _content.Children.Add(SectionTitle("Backup disponibili"));
        foreach (var b in _backupService.GetBackups())
        {
            _content.Children.Add(new TextBlock
            {
                Text = $"{b.Name} - {b.Length} byte - {b.CreationTime}"
            });
        }
    }

    private static TextBlock SectionTitle(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 18,
            FontWeight = FontWeight.Bold,
            Foreground = Brush.Parse("#B5162B"),
            Margin = new Thickness(0, 8, 0, 0)
        };
    }
}
