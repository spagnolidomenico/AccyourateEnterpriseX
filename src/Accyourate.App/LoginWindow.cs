using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Accyourate.App.Models;
using Accyourate.App.Security;
using System;

namespace Accyourate.App;

public sealed class LoginWindow : Window
{
    private readonly AuthenticationService _auth;
    private readonly TextBox _usernameBox;
    private readonly TextBox _passwordBox;
    private readonly TextBlock _errorText;

    public event Action<CurrentUser>? LoginSucceeded;

    public LoginWindow(AuthenticationService auth)
    {
        _auth = auth;
        Title = "Accyourate Enterprise X - Login";
        Width = 520;
        Height = 560;
        
        MinWidth = 1024;
        MinHeight = 680;
CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Background = Brush.Parse("#F7F7F6");

        var root = new Grid
        {
            Margin = new Thickness(34),
            RowDefinitions = new RowDefinitions("*,Auto,*")
        };

        var card = new Border
        {
            Background = Brushes.White,
            CornerRadius = new CornerRadius(18),
            Padding = new Thickness(28),
            BoxShadow = BoxShadows.Parse("0 8 24 0 #20000000"),
            VerticalAlignment = VerticalAlignment.Center
        };

        var stack = new StackPanel
        {
            Spacing = 12,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        stack.Children.Add(new TextBlock
        {
            Text = "ACCYOURATE ENTERPRISE X",
            Foreground = Brush.Parse("#B5162B"),
            FontSize = 25,
            FontWeight = FontWeight.Bold,
            HorizontalAlignment = HorizontalAlignment.Center
        });

        stack.Children.Add(new TextBlock
        {
            Text = "2026 Compilable Base",
            Foreground = Brush.Parse("#2B2926"),
            FontSize = 14,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 18)
        });

        stack.Children.Add(new TextBlock { Text = "Username" });

        _usernameBox = new TextBox
        {
            Text = "admin",
            Watermark = "Username"
        };
        stack.Children.Add(_usernameBox);

        stack.Children.Add(new TextBlock { Text = "Password" });

        _passwordBox = new TextBox
        {
            Text = "admin123",
            PasswordChar = '●',
            Watermark = "Password"
        };
        stack.Children.Add(_passwordBox);

        _errorText = new TextBlock
        {
            Foreground = Brush.Parse("#B5162B"),
            Text = "",
            MinHeight = 22
        };
        stack.Children.Add(_errorText);

        var button = new Button
        {
            Content = "Accedi",
            Background = Brush.Parse("#B5162B"),
            Foreground = Brushes.White,
            FontWeight = FontWeight.Bold,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Padding = new Thickness(12)
        };
        button.Click += (_, _) => Login();
        stack.Children.Add(button);

        stack.Children.Add(new TextBlock
        {
            Text = "Credenziali iniziali: admin / admin123",
            Foreground = Brush.Parse("#666666"),
            FontSize = 12,
            HorizontalAlignment = HorizontalAlignment.Center
        });

        card.Child = stack;
        Grid.SetRow(card, 1);
        root.Children.Add(card);

        Content = root;
    }

    private void Login()
    {
        var username = _usernameBox.Text?.Trim() ?? "";
        var password = _passwordBox.Text ?? "";

        var user = _auth.Login(username, password);

        if (user is not null)
        {
            LoginSucceeded?.Invoke(user);
            return;
        }

        _errorText.Text = "Username o password non validi.";
    }
}
