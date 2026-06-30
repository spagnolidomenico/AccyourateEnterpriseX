using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Accyourate.App.Data;
using Accyourate.App.Models;

namespace Accyourate.App;

public sealed class UsersWindow : Window
{
    private readonly DatabaseService _database;
    private readonly CurrentUser _currentUser;
    private readonly StackPanel _usersPanel = new();
    private readonly TextBlock _message = new();
    private readonly TextBox _username = new();
    private readonly TextBox _displayName = new();
    private readonly TextBox _password = new();
    private readonly ComboBox _role = new();

    public UsersWindow(DatabaseService database, CurrentUser currentUser)
    {
        _database = database;
        _currentUser = currentUser;

        Title = "Accyourate Enterprise X - Gestione Utenti";
        Width = 980;
        Height = 680;
        
        MinWidth = 1024;
        MinHeight = 680;
WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = Brush.Parse("#F7F7F6");

        Content = BuildLayout();
        RefreshUsers();
    }

    private Control BuildLayout()
    {
        var root = new ScrollViewer
        {
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        };
        var stack = new StackPanel { Margin = new Thickness(24), Spacing = 16 };

        stack.Children.Add(new TextBlock
        {
            Text = "Gestione Utenti",
            FontSize = 26,
            FontWeight = FontWeight.Bold,
            Foreground = Brush.Parse("#B5162B")
        });

        stack.Children.Add(new TextBlock
        {
            Text = "Developer Edition 1.0: creazione utenti, cambio ruolo, attiva/disattiva."
        });

        stack.Children.Add(new Border
        {
            Background = Brushes.White,
            CornerRadius = new CornerRadius(14),
            Padding = new Thickness(18),
            Child = BuildCreateForm()
        });

        _message.Foreground = Brush.Parse("#B5162B");
        stack.Children.Add(_message);

        stack.Children.Add(new Border
        {
            Background = Brushes.White,
            CornerRadius = new CornerRadius(14),
            Padding = new Thickness(18),
            Child = _usersPanel
        });

        root.Content = stack;
        return root;
    }

    private Control BuildCreateForm()
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*,*,180,120"),
            RowDefinitions = new RowDefinitions("Auto,Auto")
        };

        AddLabel(grid, "Username", 0, 0);
        AddLabel(grid, "Nome visualizzato", 1, 0);
        AddLabel(grid, "Password", 2, 0);
        AddLabel(grid, "Ruolo", 3, 0);

        _username.Watermark = "es. mario.rossi";
        _displayName.Watermark = "Mario Rossi";
        _password.Watermark = "Password";
        _password.PasswordChar = '●';
        _role.ItemsSource = new[] { "Admin", "Operatore", "Lettura" };
        _role.SelectedIndex = 1;

        AddControl(grid, _username, 0, 1);
        AddControl(grid, _displayName, 1, 1);
        AddControl(grid, _password, 2, 1);
        AddControl(grid, _role, 3, 1);

        var create = new Button
        {
            Content = "Crea utente",
            Background = Brush.Parse("#B5162B"),
            Foreground = Brushes.White,
            FontWeight = FontWeight.Bold,
            VerticalAlignment = VerticalAlignment.Bottom
        };
        create.Click += (_, _) => CreateUser();
        AddControl(grid, create, 4, 1);

        return grid;
    }

    private static void AddLabel(Grid grid, string text, int column, int row)
    {
        var label = new TextBlock
        {
            Text = text,
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(4)
        };
        AddControl(grid, label, column, row);
    }

    private static void AddControl(Grid grid, Control control, int column, int row)
    {
        control.Margin = new Thickness(4);
        Grid.SetColumn(control, column);
        Grid.SetRow(control, row);
        grid.Children.Add(control);
    }

    private void CreateUser()
    {
        var role = _role.SelectedItem?.ToString() ?? "Operatore";
        var ok = _database.CreateUser(
            _username.Text ?? "",
            _displayName.Text ?? "",
            _password.Text ?? "",
            role,
            _currentUser.Username,
            out var error);

        if (!ok)
        {
            _message.Text = error;
            return;
        }

        _message.Text = "Utente creato correttamente.";
        _username.Text = "";
        _displayName.Text = "";
        _password.Text = "";
        RefreshUsers();
    }

    private void RefreshUsers()
    {
        _usersPanel.Children.Clear();
        _usersPanel.Spacing = 8;

        _usersPanel.Children.Add(new TextBlock
        {
            Text = "Utenti registrati",
            FontSize = 18,
            FontWeight = FontWeight.Bold
        });

        var header = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("160,220,120,90,100,100")
        };
        AddHeader(header, "Username", 0);
        AddHeader(header, "Nome", 1);
        AddHeader(header, "Ruolo", 2);
        AddHeader(header, "Stato", 3);
        AddHeader(header, "Cambia ruolo", 4);
        AddHeader(header, "Attiva/Disattiva", 5);
        _usersPanel.Children.Add(header);

        foreach (var user in _database.GetUsers())
        {
            _usersPanel.Children.Add(BuildUserRow(user));
        }
    }

    private static void AddHeader(Grid grid, string text, int column)
    {
        var block = new TextBlock
        {
            Text = text,
            FontWeight = FontWeight.Bold,
            Foreground = Brush.Parse("#2B2926"),
            Margin = new Thickness(4)
        };
        AddControl(grid, block, column, 0);
    }

    private Control BuildUserRow(UserRecord user)
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("160,220,120,90,100,100"),
            Margin = new Thickness(0, 4)
        };

        AddText(grid, user.Username, 0);
        AddText(grid, user.DisplayName, 1);
        AddText(grid, user.Role, 2);
        AddText(grid, user.IsActive ? "Attivo" : "Disattivo", 3);

        var roleButton = new Button { Content = user.Role == "Admin" ? "Operatore" : "Admin", Margin = new Thickness(4) };
        roleButton.Click += (_, _) =>
        {
            var newRole = user.Role == "Admin" ? "Operatore" : "Admin";
            _database.SetUserRole(user.Id, newRole, _currentUser.Username);
            RefreshUsers();
        };
        AddControl(grid, roleButton, 4, 0);

        var activeButton = new Button { Content = user.IsActive ? "Disattiva" : "Attiva", Margin = new Thickness(4) };
        activeButton.Click += (_, _) =>
        {
            if (user.Username == "admin" && user.IsActive)
            {
                _message.Text = "L'utente admin principale non può essere disattivato.";
                return;
            }

            _database.SetUserActive(user.Id, !user.IsActive, _currentUser.Username);
            RefreshUsers();
        };
        AddControl(grid, activeButton, 5, 0);

        return grid;
    }

    private static void AddText(Grid grid, string text, int column)
    {
        var block = new TextBlock
        {
            Text = text,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(4)
        };
        AddControl(grid, block, column, 0);
    }
}
