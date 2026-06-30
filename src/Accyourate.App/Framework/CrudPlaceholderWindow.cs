using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.Models;

namespace Accyourate.App.Framework;

public sealed class CrudPlaceholderWindow : Window
{
    private readonly CrudModuleDefinition _module;
    private readonly CurrentUser _user;

    public CrudPlaceholderWindow(CrudModuleDefinition module, CurrentUser user)
    {
        _module = module;
        _user = user;

        Title = $"Accyourate Enterprise X - {_module.Title}";
        Width = 1100;
        Height = 720;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = Brush.Parse("#F7F7F6");

        Content = BuildLayout();
    }

    private Control BuildLayout()
    {
        var root = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,Auto,*")
        };

        var title = new StackPanel
        {
            Margin = new Thickness(24, 20, 24, 10),
            Spacing = 4
        };

        title.Children.Add(new TextBlock
        {
            Text = _module.Title,
            FontSize = 28,
            FontWeight = FontWeight.Bold,
            Foreground = Brush.Parse("#B5162B")
        });

        title.Children.Add(new TextBlock
        {
            Text = $"Modulo: {_module.Code} • Utente: {_user.DisplayName}",
            Foreground = Brush.Parse("#555555")
        });

        Grid.SetRow(title, 0);
        root.Children.Add(title);

        var actions = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Margin = new Thickness(24, 0, 24, 12),
            Spacing = 8
        };

        foreach (var action in _module.Actions)
        {
            if (!_user.Can(action.Permission))
                continue;

            actions.Children.Add(new Button
            {
                Content = action.Title,
                Padding = new Thickness(12, 8),
                Background = action.Code == "new" ? Brush.Parse("#B5162B") : Brushes.White,
                Foreground = action.Code == "new" ? Brushes.White : Brush.Parse("#2B2926")
            });
        }

        Grid.SetRow(actions, 1);
        root.Children.Add(actions);

        var card = new Border
        {
            Background = Brushes.White,
            CornerRadius = new CornerRadius(16),
            Padding = new Thickness(22),
            Margin = new Thickness(24),
            Child = BuildContent()
        };

        Grid.SetRow(card, 2);
        root.Children.Add(card);

        return root;
    }

    private Control BuildContent()
    {
        var stack = new StackPanel { Spacing = 12 };

        stack.Children.Add(new TextBlock
        {
            Text = "Framework CRUD predisposto",
            FontSize = 20,
            FontWeight = FontWeight.Bold
        });

        stack.Children.Add(new TextBlock
        {
            Text = "Questa schermata è il modello comune che verrà usato dai moduli reali."
        });

        stack.Children.Add(new TextBlock
        {
            Text = "Funzioni standard già previste: Nuovo, Modifica, Archivia, QR Code, Etichetta, Excel, Storico."
        });

        stack.Children.Add(new Separator());

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("150,220,160,160,120"),
            RowDefinitions = new RowDefinitions("Auto,Auto,Auto,Auto")
        };

        AddCell(grid, "Codice", 0, 0, true);
        AddCell(grid, "Descrizione", 1, 0, true);
        AddCell(grid, "Categoria", 2, 0, true);
        AddCell(grid, "Stato", 3, 0, true);
        AddCell(grid, "Azioni", 4, 0, true);

        AddCell(grid, $"{_module.Code.ToUpper()}-001", 0, 1);
        AddCell(grid, "Record dimostrativo", 1, 1);
        AddCell(grid, _module.Title, 2, 1);
        AddCell(grid, "Attivo", 3, 1);
        AddCell(grid, "Apri / QR", 4, 1);

        AddCell(grid, $"{_module.Code.ToUpper()}-002", 0, 2);
        AddCell(grid, "Secondo record demo", 1, 2);
        AddCell(grid, _module.Title, 2, 2);
        AddCell(grid, "Da completare", 3, 2);
        AddCell(grid, "Apri / QR", 4, 2);

        stack.Children.Add(grid);
        return stack;
    }

    private static void AddCell(Grid grid, string text, int column, int row, bool header = false)
    {
        var block = new TextBlock
        {
            Text = text,
            FontWeight = header ? FontWeight.Bold : FontWeight.Normal,
            Margin = new Thickness(6),
            Foreground = header ? Brush.Parse("#B5162B") : Brush.Parse("#2B2926")
        };

        Grid.SetColumn(block, column);
        Grid.SetRow(block, row);
        grid.Children.Add(block);
    }
}
