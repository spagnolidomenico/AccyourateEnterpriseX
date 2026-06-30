using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.Data;
using Accyourate.App.Models;

namespace Accyourate.App;

public sealed class BrandingCenterWindow : Window
{
    private readonly DatabaseService _database;
    private readonly CurrentUser _user;
    private readonly TextBlock _message = new();

    private readonly TextBox _companyName = new();
    private readonly TextBox _productTitle = new();
    private readonly TextBox _heroTitle = new();
    private readonly TextBox _heroSubtitle = new();
    private readonly TextBox _heroImagePath = new();
    private readonly TextBox _logoPath = new();
    private readonly TextBox _industryLabel = new();

    public BrandingCenterWindow(DatabaseService database, CurrentUser user)
    {
        _database = database;
        _user = user;

        Title = "Accyourate Enterprise X - Branding Center";
        Width = 1080;
        Height = 780;
        MinWidth = 980;
        MinHeight = 700;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = Brush.Parse("#F5F5F7");
        Content = BuildLayout();
        Load();
    }

    private Control BuildLayout()
    {
        var page = new StackPanel { Margin = new Thickness(24), Spacing = 14 };

        page.Children.Add(new TextBlock
        {
            Text = "Branding Center",
            FontSize = 30,
            FontWeight = FontWeight.Bold,
            Foreground = Brush.Parse("#1D1D1F")
        });

        page.Children.Add(new TextBlock
        {
            Text = "Personalizza immagine principale, logo, messaggio e identità aziendale della schermata iniziale.",
            TextWrapping = TextWrapping.Wrap,
            Foreground = Brush.Parse("#6E6E73")
        });

        var form = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*")
        };

        var left = new StackPanel { Spacing = 10 };
        left.Children.Add(Label("Nome azienda"));
        left.Children.Add(_companyName);
        left.Children.Add(Label("Titolo prodotto"));
        left.Children.Add(_productTitle);
        left.Children.Add(Label("Titolo hero"));
        left.Children.Add(_heroTitle);
        left.Children.Add(Label("Sottotitolo hero"));
        left.Children.Add(_heroSubtitle);

        var right = new StackPanel { Spacing = 10 };
        right.Children.Add(Label("Percorso immagine hero"));
        _heroImagePath.Watermark = "C:\\Immagini\\capo_tessile_ecg.png";
        right.Children.Add(_heroImagePath);
        right.Children.Add(Label("Percorso logo"));
        _logoPath.Watermark = "C:\\Logo\\logo.png";
        right.Children.Add(_logoPath);
        right.Children.Add(Label("Etichetta settore"));
        right.Children.Add(_industryLabel);

        var save = new Button
        {
            Content = "Salva branding",
            Background = Brush.Parse("#0A84FF"),
            Foreground = Brushes.White,
            FontWeight = FontWeight.Bold,
            Padding = new Thickness(14, 10),
            CornerRadius = new CornerRadius(12)
        };
        save.Click += (_, _) => Save();
        right.Children.Add(save);

        var preview = new Button
        {
            Content = "Apri anteprima Splash/Login",
            Background = Brush.Parse("#F2F2F7"),
            Foreground = Brush.Parse("#1D1D1F"),
            Padding = new Thickness(14, 10),
            CornerRadius = new CornerRadius(12)
        };
        preview.Click += (_, _) => new BrandedSplashLoginWindow(_database, _user).Show();
        right.Children.Add(preview);

        _message.Foreground = Brush.Parse("#0A84FF");
        right.Children.Add(_message);

        Add(form, Card(left), 0, 0);
        Add(form, Card(right), 1, 0);
        page.Children.Add(form);

        page.Children.Add(Card(new TextBlock
        {
            Text = "Suggerimento: per il tuo caso puoi usare un'immagine di capo tessile intelligente con monitoraggio cardiaco ed elettrocardiogramma, così l'utente capisce subito il dominio applicativo.",
            TextWrapping = TextWrapping.Wrap,
            Foreground = Brush.Parse("#6E6E73")
        }));

        return new ScrollViewer
        {
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            Content = page
        };
    }

    private void Load()
    {
        var b = _database.GetBrandingPreferences();
        _companyName.Text = b.CompanyName;
        _productTitle.Text = b.ProductTitle;
        _heroTitle.Text = b.HeroTitle;
        _heroSubtitle.Text = b.HeroSubtitle;
        _heroImagePath.Text = b.HeroImagePath;
        _logoPath.Text = b.LogoPath;
        _industryLabel.Text = b.IndustryLabel;
    }

    private void Save()
    {
        var b = new BrandingPreferenceRecord
        {
            CompanyName = _companyName.Text ?? "",
            ProductTitle = _productTitle.Text ?? "",
            HeroTitle = _heroTitle.Text ?? "",
            HeroSubtitle = _heroSubtitle.Text ?? "",
            HeroImagePath = _heroImagePath.Text ?? "",
            LogoPath = _logoPath.Text ?? "",
            IndustryLabel = _industryLabel.Text ?? ""
        };

        _database.SaveBrandingPreferences(b, _user.Username);
        _message.Text = "Branding salvato.";
    }

    private static TextBlock Label(string text) => new()
    {
        Text = text,
        FontWeight = FontWeight.Bold,
        Foreground = Brush.Parse("#1D1D1F")
    };

    private static Border Card(Control child) => new()
    {
        Background = Brushes.White,
        CornerRadius = new CornerRadius(16),
        Padding = new Thickness(18),
        Margin = new Thickness(6),
        Child = child
    };

    private static void Add(Grid grid, Control control, int column, int row)
    {
        Grid.SetColumn(control, column);
        Grid.SetRow(control, row);
        grid.Children.Add(control);
    }
}
