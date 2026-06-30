using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.Data;
using Accyourate.App.Models;

namespace Accyourate.App;

public sealed class ChangePasswordWindow : Window
{
    private readonly DatabaseService _database;
    private readonly CurrentUser _user;

    private readonly TextBox _oldPassword = new();
    private readonly TextBox _newPassword = new();
    private readonly TextBox _confirmPassword = new();
    private readonly TextBlock _message = new();

    public ChangePasswordWindow(DatabaseService database, CurrentUser user)
    {
        _database = database;
        _user = user;

        Title = "Accyourate Enterprise X - Cambio Password";
        Width = 520;
        Height = 420;
        
        MinWidth = 1024;
        MinHeight = 680;
WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = Brush.Parse("#F7F7F6");

        Content = BuildLayout();
    }

    private Control BuildLayout()
    {
        var stack = new StackPanel { Margin = new Thickness(28), Spacing = 12 };

        stack.Children.Add(new TextBlock
        {
            Text = "Cambio Password",
            FontSize = 24,
            FontWeight = FontWeight.Bold,
            Foreground = Brush.Parse("#B5162B")
        });

        _oldPassword.PasswordChar = '●';
        _newPassword.PasswordChar = '●';
        _confirmPassword.PasswordChar = '●';

        stack.Children.Add(new TextBlock { Text = "Password attuale" });
        stack.Children.Add(_oldPassword);

        stack.Children.Add(new TextBlock { Text = "Nuova password" });
        stack.Children.Add(_newPassword);

        stack.Children.Add(new TextBlock { Text = "Conferma nuova password" });
        stack.Children.Add(_confirmPassword);

        _message.Foreground = Brush.Parse("#B5162B");
        stack.Children.Add(_message);

        var save = new Button
        {
            Content = "Salva password",
            Background = Brush.Parse("#B5162B"),
            Foreground = Brushes.White,
            FontWeight = FontWeight.Bold,
            Padding = new Thickness(12)
        };
        save.Click += (_, _) => Save();
        stack.Children.Add(save);

        return new Border
        {
            Background = Brushes.White,
            CornerRadius = new CornerRadius(16),
            Padding = new Thickness(18),
            Margin = new Thickness(20),
            Child = stack
        };
    }

    private void Save()
    {
        if ((_newPassword.Text ?? "") != (_confirmPassword.Text ?? ""))
        {
            _message.Text = "Le nuove password non coincidono.";
            return;
        }

        var ok = _database.ChangePassword(
            _user.Username,
            _oldPassword.Text ?? "",
            _newPassword.Text ?? "",
            out var error);

        if (!ok)
        {
            _message.Text = error;
            return;
        }

        _message.Text = "Password cambiata correttamente.";
        _oldPassword.Text = "";
        _newPassword.Text = "";
        _confirmPassword.Text = "";
    }
}
