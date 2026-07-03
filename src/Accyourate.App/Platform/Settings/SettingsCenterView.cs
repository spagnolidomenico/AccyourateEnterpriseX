using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App.Platform.Settings;

public sealed class SettingsCenterView : UserControl
{
    private readonly SettingsService _settingsService = new();
    private ApplicationSettings _settings = new();

    private readonly TextBlock _message = new();

    private readonly TextBox _companyName = new();
    private readonly TextBox _legalName = new();
    private readonly TextBox _vatNumber = new();
    private readonly TextBox _fiscalCode = new();
    private readonly TextBox _address = new();
    private readonly TextBox _city = new();
    private readonly TextBox _province = new();
    private readonly TextBox _country = new();
    private readonly TextBox _phone = new();
    private readonly TextBox _email = new();
    private readonly TextBox _pec = new();
    private readonly TextBox _website = new();
    private readonly TextBox _logoPath = new();

    private readonly TextBox _employeePrefix = new();
    private readonly TextBox _assetPrefix = new();
    private readonly TextBox _deliveryReportPrefix = new();
    private readonly TextBox _documentPrefix = new();
    private readonly TextBox _padding = new();
    private readonly CheckBox _includeYearDeliveryReports = new();

    private readonly TextBox _documentRoot = new();
    private readonly TextBox _deliveryFolder = new();
    private readonly TextBox _hrFolder = new();
    private readonly TextBox _assetFolder = new();

    public SettingsCenterView()
    {
        Background = UiTokens.Brush(UiTokens.Background);
        Content = BuildLayout();
        Load();
    }

    private Control BuildLayout()
    {
        var root = new DockPanel();

        var header = new Grid
        {
            Margin = new Thickness(24, 20, 24, 12),
            ColumnDefinitions = new ColumnDefinitions("*,120,120")
        };

        var title = new StackPanel { Spacing = 4 };
        title.Children.Add(new TextBlock
        {
            Text = "Impostazioni",
            FontSize = 32,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });
        title.Children.Add(new TextBlock
        {
            Text = "Configurazione centrale di azienda, numerazioni e documenti.",
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            TextWrapping = TextWrapping.Wrap
        });

        Add(header, title, 0, 0);
        Add(header, ToolbarButton("↻ Ricarica", Load, false), 1, 0);
        Add(header, ToolbarButton("Salva", Save, true), 2, 0);

        DockPanel.SetDock(header, Dock.Top);
        root.Children.Add(header);

        _message.Margin = new Thickness(24, 0, 24, 12);
        _message.TextWrapping = TextWrapping.Wrap;
        _message.Foreground = UiTokens.Brush(UiTokens.BrandBlue);
        DockPanel.SetDock(_message, Dock.Top);
        root.Children.Add(_message);

        var content = new StackPanel
        {
            Margin = new Thickness(24, 0, 24, 24),
            Spacing = 14
        };

        content.Children.Add(Section("Azienda", new[]
        {
            Field("Nome azienda", _companyName),
            Field("Ragione sociale", _legalName),
            Field("Partita IVA", _vatNumber),
            Field("Codice fiscale", _fiscalCode),
            Field("Indirizzo", _address),
            Field("Città", _city),
            Field("Provincia", _province),
            Field("Paese", _country),
            Field("Telefono", _phone),
            Field("Email", _email),
            Field("PEC", _pec),
            Field("Sito web", _website),
            Field("Percorso logo", _logoPath)
        }));

        _includeYearDeliveryReports.Content = "Includi anno nei verbali";

        content.Children.Add(Section("Numerazioni", new Control[]
        {
            Field("Prefisso dipendenti", _employeePrefix),
            Field("Prefisso asset", _assetPrefix),
            Field("Prefisso verbali", _deliveryReportPrefix),
            Field("Prefisso documenti", _documentPrefix),
            Field("Numero cifre", _padding),
            _includeYearDeliveryReports
        }));

        content.Children.Add(Section("Documenti", new[]
        {
            Field("Cartella principale documenti", _documentRoot),
            Field("Cartella verbali consegna", _deliveryFolder),
            Field("Cartella documenti HR", _hrFolder),
            Field("Cartella documenti Asset", _assetFolder)
        }));

        content.Children.Add(Section("Informazioni", new[]
        {
            Info("File impostazioni", _settingsService.SettingsPath),
            Info("Canale versione", "Beta"),
            Info("Prossimi step", "Collegamento PDF, Branding Center e Backup.")
        }));

        root.Children.Add(new ScrollViewer
        {
            Content = content,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        });

        return root;
    }

    private void Load()
    {
        try
        {
            _settings = _settingsService.Load();

            _companyName.Text = _settings.Company.CompanyName;
            _legalName.Text = _settings.Company.LegalName;
            _vatNumber.Text = _settings.Company.VatNumber;
            _fiscalCode.Text = _settings.Company.FiscalCode;
            _address.Text = _settings.Company.Address;
            _city.Text = _settings.Company.City;
            _province.Text = _settings.Company.Province;
            _country.Text = _settings.Company.Country;
            _phone.Text = _settings.Company.Phone;
            _email.Text = _settings.Company.Email;
            _pec.Text = _settings.Company.Pec;
            _website.Text = _settings.Company.Website;
            _logoPath.Text = _settings.Company.LogoPath;

            _employeePrefix.Text = _settings.Numbering.EmployeePrefix;
            _assetPrefix.Text = _settings.Numbering.AssetPrefix;
            _deliveryReportPrefix.Text = _settings.Numbering.DeliveryReportPrefix;
            _documentPrefix.Text = _settings.Numbering.DocumentPrefix;
            _padding.Text = _settings.Numbering.Padding.ToString();
            _includeYearDeliveryReports.IsChecked = _settings.Numbering.IncludeYearInDeliveryReports;

            _documentRoot.Text = _settings.Documents.DocumentRootPath;
            _deliveryFolder.Text = _settings.Documents.DeliveryReportsFolderName;
            _hrFolder.Text = _settings.Documents.HrDocumentsFolderName;
            _assetFolder.Text = _settings.Documents.AssetDocumentsFolderName;

            ShowMessage("Impostazioni caricate.");
        }
        catch (Exception ex)
        {
            ShowMessage($"Errore caricamento impostazioni: {ex.Message}", true);
        }
    }

    private void Save()
    {
        try
        {
            _settings.Company.CompanyName = Text(_companyName);
            _settings.Company.LegalName = Text(_legalName);
            _settings.Company.VatNumber = Text(_vatNumber);
            _settings.Company.FiscalCode = Text(_fiscalCode);
            _settings.Company.Address = Text(_address);
            _settings.Company.City = Text(_city);
            _settings.Company.Province = Text(_province);
            _settings.Company.Country = Text(_country);
            _settings.Company.Phone = Text(_phone);
            _settings.Company.Email = Text(_email);
            _settings.Company.Pec = Text(_pec);
            _settings.Company.Website = Text(_website);
            _settings.Company.LogoPath = Text(_logoPath);

            _settings.Numbering.EmployeePrefix = Text(_employeePrefix);
            _settings.Numbering.AssetPrefix = Text(_assetPrefix);
            _settings.Numbering.DeliveryReportPrefix = Text(_deliveryReportPrefix);
            _settings.Numbering.DocumentPrefix = Text(_documentPrefix);
            _settings.Numbering.Padding = int.TryParse(Text(_padding), out var padding) ? Math.Clamp(padding, 3, 10) : 6;
            _settings.Numbering.IncludeYearInDeliveryReports = _includeYearDeliveryReports.IsChecked == true;

            _settings.Documents.DocumentRootPath = Text(_documentRoot);
            _settings.Documents.DeliveryReportsFolderName = Text(_deliveryFolder);
            _settings.Documents.HrDocumentsFolderName = Text(_hrFolder);
            _settings.Documents.AssetDocumentsFolderName = Text(_assetFolder);

            _settingsService.Save(_settings);
            ShowMessage("Impostazioni salvate correttamente.");
        }
        catch (Exception ex)
        {
            ShowMessage($"Errore salvataggio impostazioni: {ex.Message}", true);
        }
    }

    private static string Text(TextBox textBox) => (textBox.Text ?? string.Empty).Trim();

    private void ShowMessage(string message, bool isError = false)
    {
        _message.Text = message;
        _message.Foreground = UiTokens.Brush(isError ? UiTokens.Danger : UiTokens.BrandBlue);
    }

    private static Control Section(string title, IEnumerable<Control> fields)
    {
        var stack = new WrapPanel
        {
            ItemWidth = 320,
            ItemHeight = 74
        };

        foreach (var field in fields)
            stack.Children.Add(field);

        var root = new StackPanel { Spacing = 10 };
        root.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 20,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });
        root.Children.Add(stack);

        return Card(root);
    }

    private static Control Field(string label, TextBox input)
    {
        input.Margin = new Thickness(0, 0, 12, 0);

        return new StackPanel
        {
            Spacing = 4,
            Margin = new Thickness(0, 0, 12, 0),
            Children =
            {
                new TextBlock
                {
                    Text = label,
                    FontSize = 12,
                    FontWeight = FontWeight.SemiBold,
                    Foreground = UiTokens.Brush(UiTokens.TextSecondary)
                },
                input
            }
        };
    }

    private static Control Info(string label, string value)
    {
        return new Border
        {
            Background = UiTokens.Brush(UiTokens.SurfaceAlt),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(12),
            Margin = new Thickness(0, 0, 12, 0),
            Child = new StackPanel
            {
                Spacing = 4,
                Children =
                {
                    new TextBlock
                    {
                        Text = label,
                        FontSize = 12,
                        Foreground = UiTokens.Brush(UiTokens.TextSecondary)
                    },
                    new TextBlock
                    {
                        Text = string.IsNullOrWhiteSpace(value) ? "—" : value,
                        TextWrapping = TextWrapping.Wrap,
                        FontWeight = FontWeight.SemiBold,
                        Foreground = UiTokens.Brush(UiTokens.TextPrimary)
                    }
                }
            }
        };
    }

    private static Border Card(Control child)
    {
        return new Border
        {
            Background = UiTokens.Brush(UiTokens.Surface),
            CornerRadius = new CornerRadius(22),
            Padding = new Thickness(18),
            Child = child
        };
    }

    private static Button ToolbarButton(string text, Action action, bool primary)
    {
        var b = new Button
        {
            Content = text,
            Background = UiTokens.Brush(primary ? UiTokens.BrandBlue : UiTokens.Surface),
            Foreground = primary ? Brushes.White : UiTokens.Brush(UiTokens.TextPrimary),
            FontWeight = primary ? FontWeight.Bold : FontWeight.Normal,
            Padding = new Thickness(12, 8),
            CornerRadius = new CornerRadius(12),
            Margin = new Thickness(8, 0, 0, 0)
        };

        b.Click += (_, _) => action();
        return b;
    }

    private static void Add(Grid grid, Control control, int column, int row)
    {
        Grid.SetColumn(control, column);
        Grid.SetRow(control, row);
        grid.Children.Add(control);
    }
}
