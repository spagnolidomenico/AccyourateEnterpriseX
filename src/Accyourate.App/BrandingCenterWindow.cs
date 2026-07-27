using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Accyourate.App.Data;
using Accyourate.App.Models;
using Accyourate.App.Platform.Pdf;
using Accyourate.App.Platform.Settings;
using Accyourate.App.UIFramework.Tokens;

namespace Accyourate.App;

public sealed class BrandingCenterWindow : Window
{
    private readonly DatabaseService _database;
    private readonly CurrentUser _user;
    private readonly SettingsService _settingsService = new();
    private ApplicationSettings _settings = new();

    private readonly TextBlock _message = new();
    private readonly TextBlock _previewCompany = new();
    private readonly TextBlock _previewDetails = new();
    private readonly TextBlock _previewDocument = new();
    private readonly Border _previewHeader = new();
    private readonly Image _previewLogoImage = new() { Stretch = Stretch.Uniform, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center };
    private readonly TextBlock _previewLogoPlaceholder = new() { Text = "LOGO", FontWeight = FontWeight.Bold, VerticalAlignment = VerticalAlignment.Center };
    private readonly Image _companyLogoPreview = new() { Stretch = Stretch.Uniform, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
    private readonly TextBlock _companyLogoPlaceholder = new() { Text = "Nessun logo selezionato", HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
    private readonly TextBlock _previewFooter = new();
    private readonly StackPanel _previewSignatures = new();

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

    private readonly TextBox _templateName = new();
    private readonly ComboBox _headerLayout = new();
    private readonly ComboBox _logoSize = new();
    private readonly ComboBox _logoPosition = new();
    private readonly TextBox _primaryColor = new();
    private readonly TextBox _secondaryColor = new();
    private readonly TextBox _footerText = new();
    private readonly TextBox _leftSignature = new();
    private readonly TextBox _rightSignature = new();
    private readonly CheckBox _showLogo = new() { Content = "Mostra logo" };
    private readonly CheckBox _showCompanyDetails = new() { Content = "Mostra dati aziendali" };
    private readonly CheckBox _showMetadata = new() { Content = "Mostra codice e data documento" };
    private readonly CheckBox _showFooter = new() { Content = "Mostra piè di pagina" };
    private readonly CheckBox _showSignatures = new() { Content = "Mostra firme" };
    private readonly CheckBox _showQr = new() { Content = "Riserva spazio QR Code" };

    public BrandingCenterWindow(DatabaseService database, CurrentUser user)
    {
        _database = database;
        _user = user;

        Title = "Accyourate Enterprise X - Branding & Template Designer";
        Width = 1280;
        Height = 840;
        MinWidth = 1040;
        MinHeight = 720;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = UiTokens.Brush(UiTokens.Background);
        Content = BuildLayout();
        HookPreviewEvents();
        Load();
    }

    private void HookPreviewEvents()
    {
        foreach (var box in new[] { _companyName, _legalName, _vatNumber, _fiscalCode, _address, _city, _province, _country, _phone, _email, _pec, _website, _primaryColor, _footerText, _leftSignature, _rightSignature })
            box.TextChanged += (_, _) => RefreshPreview();
        _headerLayout.SelectionChanged += (_, _) => RefreshPreview();
        _logoSize.SelectionChanged += (_, _) => RefreshPreview();
        _logoPosition.SelectionChanged += (_, _) => RefreshPreview();
        foreach (var check in new[] { _showLogo, _showCompanyDetails, _showMetadata, _showFooter, _showSignatures, _showQr })
            check.Click += (_, _) => RefreshPreview();
    }

    private Control BuildLayout()
    {
        _headerLayout.ItemsSource = new[] { "Corporate", "Enterprise", "Compatta" };
        _logoSize.ItemsSource = new[] { "Piccolo", "Medio", "Grande" };
        _logoPosition.ItemsSource = new[] { "Sinistra", "Centro", "Destra" };

        var root = new DockPanel();
        var header = new Grid
        {
            Margin = new Thickness(24, 20, 24, 12),
            ColumnDefinitions = new ColumnDefinitions("*,130,130,150")
        };

        var title = new StackPanel { Spacing = 3 };
        title.Children.Add(new TextBlock
        {
            Text = "Branding & Template Designer",
            FontSize = 30,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });
        title.Children.Add(new TextBlock
        {
            Text = "Configura una sola volta l'identità aziendale e applicala a schede, verbali e report.",
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            TextWrapping = TextWrapping.Wrap
        });
        Add(header, title, 0, 0);
        Add(header, Button("↻ Ricarica", Load, false), 1, 0);
        Add(header, Button("Anteprima PDF", GeneratePreviewPdf, false), 2, 0);
        Add(header, Button("Salva modello", Save, true), 3, 0);
        DockPanel.SetDock(header, Dock.Top);
        root.Children.Add(header);

        _message.Margin = new Thickness(24, 0, 24, 10);
        _message.TextWrapping = TextWrapping.Wrap;
        DockPanel.SetDock(_message, Dock.Top);
        root.Children.Add(_message);

        var tabs = new TabControl { Margin = new Thickness(24, 0, 24, 24) };
        tabs.Items.Add(new TabItem { Header = "Azienda", Content = BuildCompanyPage() });
        tabs.Items.Add(new TabItem { Header = "Template documenti", Content = BuildTemplatePage() });
        tabs.Items.Add(new TabItem { Header = "Anteprima", Content = BuildPreviewPage() });
        root.Children.Add(tabs);
        return root;
    }

    private Control BuildCompanyPage()
    {
        var fields = new WrapPanel { ItemWidth = 330, ItemHeight = 76 };
        foreach (var field in new[]
        {
            Field("Nome commerciale", _companyName), Field("Ragione sociale", _legalName),
            Field("Partita IVA", _vatNumber), Field("Codice fiscale", _fiscalCode),
            Field("Indirizzo", _address), Field("Città", _city), Field("Provincia", _province),
            Field("Nazione", _country), Field("Telefono", _phone), Field("Email", _email),
            Field("PEC", _pec), Field("Sito web", _website)
        }) fields.Children.Add(field);

        var logoPathRow = new Grid { ColumnDefinitions = new ColumnDefinitions("*,150"), Margin = new Thickness(0, 4, 0, 0) };
        Add(logoPathRow, Field("Percorso logo aziendale", _logoPath), 0, 0);
        Add(logoPathRow, Button("Sfoglia logo", BrowseLogo, false), 1, 0);

        var logoPreviewLayer = new Grid();
        logoPreviewLayer.Children.Add(_companyLogoPlaceholder);
        logoPreviewLayer.Children.Add(_companyLogoPreview);

        var logoCard = Card(new StackPanel
        {
            Spacing = 12,
            Children =
            {
                new TextBlock { Text = "Logo aziendale", FontSize = 17, FontWeight = FontWeight.Bold },
                new TextBlock { Text = "L'anteprima mantiene sempre le proporzioni originali, senza schiacciare il logo.", Foreground = UiTokens.Brush(UiTokens.TextSecondary), TextWrapping = TextWrapping.Wrap },
                new Border
                {
                    MinWidth = 520,
                    Height = 170,
                    Background = UiTokens.Brush(UiTokens.SurfaceAlt),
                    BorderBrush = UiTokens.Brush(UiTokens.Border),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(10),
                    Padding = new Thickness(20),
                    Child = logoPreviewLayer
                },
                logoPathRow
            }
        });

        var content = new StackPanel { Spacing = 12, Margin = new Thickness(4, 16, 4, 20) };
        content.Children.Add(SectionTitle("Anagrafica aziendale", "Questi dati verranno riutilizzati automaticamente in tutti i documenti."));
        content.Children.Add(fields);
        content.Children.Add(logoCard);
        content.Children.Add(InfoCard("Formato logo", "Il logo viene adattato in modo proporzionale sia nell'anteprima sia nei PDF. Per l'incorporamento nel PDF usa JPG o JPEG; il formato PNG resta disponibile per l'anteprima nell'app."));
        return Scroll(content);
    }

    private Control BuildTemplatePage()
    {
        var content = new StackPanel { Spacing = 14, Margin = new Thickness(4, 16, 4, 20) };
        content.Children.Add(SectionTitle("Modello documento", "Definisci intestazione, colori, metadati, firme e piè di pagina."));

        var fields = new WrapPanel { ItemWidth = 330, ItemHeight = 76 };
        foreach (var field in new[]
        {
            Field("Nome modello", _templateName), Field("Modello intestazione", _headerLayout),
            Field("Dimensione logo nel PDF", _logoSize), Field("Posizione logo", _logoPosition),
            Field("Colore principale (HEX)", _primaryColor), Field("Colore secondario (HEX)", _secondaryColor),
            Field("Firma sinistra", _leftSignature), Field("Firma destra", _rightSignature)
        }) fields.Children.Add(field);
        content.Children.Add(Card(fields));

        var options = new WrapPanel { ItemWidth = 280, ItemHeight = 42 };
        foreach (var option in new Control[] { _showLogo, _showCompanyDetails, _showMetadata, _showFooter, _showSignatures, _showQr })
        {
            option.Margin = new Thickness(4);
            options.Children.Add(option);
        }
        content.Children.Add(Card(new StackPanel
        {
            Spacing = 10,
            Children =
            {
                new TextBlock { Text = "Elementi del documento", FontWeight = FontWeight.Bold, FontSize = 17 },
                options
            }
        }));

        content.Children.Add(Card(new StackPanel
        {
            Spacing = 6,
            Children =
            {
                new TextBlock { Text = "Testo piè di pagina", FontWeight = FontWeight.SemiBold },
                _footerText
            }
        }));
        content.Children.Add(InfoCard("Modelli collegati", "Il modello viene già applicato alla Scheda Asset. Nei prossimi sprint lo stesso motore verrà collegato a verbali di consegna, restituzione, registri e report."));
        return Scroll(content);
    }

    private Control BuildPreviewPage()
    {
        _previewHeader.CornerRadius = new CornerRadius(4, 4, 0, 0);
        _previewHeader.Padding = new Thickness(18, 14);
        _previewCompany.FontSize = 18;
        _previewCompany.FontWeight = FontWeight.Bold;
        _previewCompany.Foreground = Brushes.White;
        _previewDetails.FontSize = 10;
        _previewDetails.Foreground = Brushes.White;
        _previewDetails.TextWrapping = TextWrapping.Wrap;
        _previewDetails.MaxWidth = 310;
        _previewDocument.FontSize = 12;
        _previewDocument.FontWeight = FontWeight.Bold;
        _previewDocument.Foreground = Brushes.White;
        _previewDocument.TextWrapping = TextWrapping.Wrap;
        _previewDocument.TextAlignment = TextAlignment.Right;

        var previewLogoLayer = new Grid
        {
            Width = 170,
            Height = 54,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        previewLogoLayer.Children.Add(_previewLogoPlaceholder);
        previewLogoLayer.Children.Add(_previewLogoImage);

        var headerGrid = new Grid { ColumnDefinitions = new ColumnDefinitions("184,*,179") };
        previewLogoLayer.Margin = new Thickness(0, 0, 14, 0);
        Add(headerGrid, previewLogoLayer, 0, 0);
        Add(headerGrid, new StackPanel { Spacing = 3, Children = { _previewCompany, _previewDetails } }, 1, 0);
        _previewDocument.Margin = new Thickness(14, 0, 0, 0);
        Add(headerGrid, _previewDocument, 2, 0);
        _previewHeader.Child = headerGrid;

        _previewFooter.FontSize = 10;
        _previewFooter.Foreground = UiTokens.Brush(UiTokens.TextSecondary);
        _previewSignatures.Orientation = Orientation.Horizontal;
        _previewSignatures.HorizontalAlignment = HorizontalAlignment.Stretch;
        _previewSignatures.Spacing = 60;

        var paper = new Border
        {
            Width = 720,
            MinHeight = 700,
            Background = Brushes.White,
            BorderBrush = UiTokens.Brush(UiTokens.Border),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(28),
            Child = new StackPanel
            {
                Spacing = 18,
                Children =
                {
                    _previewHeader,
                    new TextBlock { Text = "SCHEDA PATRIMONIALE BENE AZIENDALE", FontSize = 21, FontWeight = FontWeight.Bold },
                    PreviewSection("Identificazione", "Codice asset: NB-001\nCategoria: Notebook\nProduttore: Dell\nModello: Latitude 7450\nSeriale: ABC123456"),
                    PreviewSection("Assegnazione", "Dipendente: Mario Rossi\nReparto: Ufficio tecnico\nData assegnazione: 27/07/2026"),
                    PreviewSection("Garanzia", "Data acquisto: 10/06/2025\nScadenza: 10/06/2028\nStato: Valida"),
                    _previewSignatures,
                    new Separator(),
                    _previewFooter
                }
            }
        };

        return Scroll(new StackPanel
        {
            Margin = new Thickness(4, 18, 4, 30),
            HorizontalAlignment = HorizontalAlignment.Center,
            Children = { paper }
        });
    }

    private async void BrowseLogo()
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Seleziona logo aziendale",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Immagini logo") { Patterns = new[] { "*.jpg", "*.jpeg", "*.png" } }
            }
        });
        var file = files.FirstOrDefault();
        if (file is null) return;
        _logoPath.Text = file.TryGetLocalPath() ?? file.Path.LocalPath;
        RefreshPreview();
    }

    private void Load()
    {
        try
        {
            _settings = _settingsService.Load();
            var company = _settings.Company;
            var template = _settings.DocumentTemplate ??= new DocumentTemplateSettings();

            _companyName.Text = company.CompanyName;
            _legalName.Text = company.LegalName;
            _vatNumber.Text = company.VatNumber;
            _fiscalCode.Text = company.FiscalCode;
            _address.Text = company.Address;
            _city.Text = company.City;
            _province.Text = company.Province;
            _country.Text = company.Country;
            _phone.Text = company.Phone;
            _email.Text = company.Email;
            _pec.Text = company.Pec;
            _website.Text = company.Website;
            _logoPath.Text = company.LogoPath;

            _templateName.Text = template.TemplateName;
            _headerLayout.SelectedItem = NormalizeHeaderLayout(template.HeaderLayout);
            _logoSize.SelectedItem = template.LogoSize;
            _logoPosition.SelectedItem = template.LogoPosition;
            _primaryColor.Text = template.PrimaryColor;
            _secondaryColor.Text = template.SecondaryColor;
            _footerText.Text = template.FooterText;
            _leftSignature.Text = template.LeftSignatureLabel;
            _rightSignature.Text = template.RightSignatureLabel;
            _showLogo.IsChecked = template.ShowLogo;
            _showCompanyDetails.IsChecked = template.ShowCompanyDetails;
            _showMetadata.IsChecked = template.ShowDocumentMetadata;
            _showFooter.IsChecked = template.ShowFooter;
            _showSignatures.IsChecked = template.ShowSignatures;
            _showQr.IsChecked = template.ShowQrCodePlaceholder;

            RefreshPreview();
            ShowMessage("Branding e modello caricati.");
        }
        catch (Exception ex)
        {
            ShowMessage($"Errore caricamento: {ex.Message}", true);
        }
    }

    private void Save()
    {
        try
        {
            var company = _settings.Company;
            company.CompanyName = Text(_companyName);
            company.LegalName = Text(_legalName);
            company.VatNumber = Text(_vatNumber);
            company.FiscalCode = Text(_fiscalCode);
            company.Address = Text(_address);
            company.City = Text(_city);
            company.Province = Text(_province);
            company.Country = Text(_country);
            company.Phone = Text(_phone);
            company.Email = Text(_email);
            company.Pec = Text(_pec);
            company.Website = Text(_website);
            company.LogoPath = Text(_logoPath);

            var template = _settings.DocumentTemplate ??= new DocumentTemplateSettings();
            template.TemplateName = Text(_templateName);
            template.HeaderLayout = _headerLayout.SelectedItem?.ToString() ?? "Corporate";
            template.LogoSize = _logoSize.SelectedItem?.ToString() ?? "Medio";
            template.LogoPosition = _logoPosition.SelectedItem?.ToString() ?? "Sinistra";
            template.PrimaryColor = NormalizeColor(Text(_primaryColor), "#0A84FF");
            template.SecondaryColor = NormalizeColor(Text(_secondaryColor), "#1D1D1F");
            template.FooterText = Text(_footerText);
            template.LeftSignatureLabel = Text(_leftSignature);
            template.RightSignatureLabel = Text(_rightSignature);
            template.ShowLogo = _showLogo.IsChecked == true;
            template.ShowCompanyDetails = _showCompanyDetails.IsChecked == true;
            template.ShowDocumentMetadata = _showMetadata.IsChecked == true;
            template.ShowFooter = _showFooter.IsChecked == true;
            template.ShowSignatures = _showSignatures.IsChecked == true;
            template.ShowQrCodePlaceholder = _showQr.IsChecked == true;

            _settingsService.Save(_settings);

            var branding = _database.GetBrandingPreferences();
            branding.CompanyName = company.CompanyName;
            branding.LogoPath = company.LogoPath;
            _database.SaveBrandingPreferences(branding, _user.Username);

            RefreshPreview();
            ShowMessage("Branding e template salvati correttamente.");
        }
        catch (Exception ex)
        {
            ShowMessage($"Errore salvataggio: {ex.Message}", true);
        }
    }

    private void GeneratePreviewPdf()
    {
        Save();
        try
        {
            var template = _settings.DocumentTemplate ?? new DocumentTemplateSettings();
            var company = _settings.Company;
            var document = new SimplePdfDocument { Title = "Anteprima Template Aziendale" };
            document.Branding.CompanyName = string.IsNullOrWhiteSpace(company.LegalName) ? company.CompanyName : company.LegalName;
            document.Branding.CompanyDetails = BuildCompanyDetails(company);
            document.Branding.CompanyDetailLines.AddRange(BuildCompanyDetailLines(company, template.HeaderLayout));
            document.Branding.HeaderLayout = template.HeaderLayout;
            document.Branding.LogoPath = company.LogoPath;
            document.Branding.LogoSize = template.LogoSize;
            document.Branding.LogoPosition = template.LogoPosition;
            document.Branding.PrimaryColor = template.PrimaryColor;
            document.Branding.DocumentLabel = "ANTEPRIMA DOCUMENTO";
            document.Branding.DocumentCode = "TPL-001";
            document.Branding.FooterText = template.FooterText;
            document.Branding.ShowLogo = template.ShowLogo;
            document.Branding.ShowCompanyDetails = template.ShowCompanyDetails;
            document.Branding.ShowDocumentMetadata = template.ShowDocumentMetadata;
            document.Branding.ShowFooter = template.ShowFooter;
            document.AddTitle("Anteprima del modello aziendale");
            document.AddHeading("Identificazione");
            document.AddText("Codice asset: NB-001");
            document.AddText("Categoria: Notebook");
            document.AddText("Produttore: Dell");
            document.AddText("Modello: Latitude 7450");
            document.AddBlank();
            document.AddHeading("Assegnazione");
            document.AddText("Dipendente: Mario Rossi");
            document.AddText("Reparto: Ufficio tecnico");
            if (template.ShowSignatures)
            {
                document.AddSignature(template.LeftSignatureLabel);
                document.AddSignature(template.RightSignatureLabel);
            }

            var folder = Path.Combine(_settings.Documents.DocumentRootPath, "Anteprime Template");
            var path = new PdfExportService().Export(document, folder, $"Anteprima_Template_{DateTime.Now:yyyyMMdd_HHmmss}");
            Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
            ShowMessage($"Anteprima PDF generata: {path}");
        }
        catch (Exception ex)
        {
            ShowMessage($"Errore generazione anteprima: {ex.Message}", true);
        }
    }

    private void RefreshPreview()
    {
        var companyName = string.IsNullOrWhiteSpace(Text(_legalName)) ? Text(_companyName) : Text(_legalName);
        _previewCompany.Text = string.IsNullOrWhiteSpace(companyName) ? "AZIENDA" : companyName;
        _previewDetails.Text = string.Join("\n", BuildCompanyDetailLinesFromInputs());
        _previewDocument.Text = _showMetadata.IsChecked == true ? "SCHEDA ASSET\nAST-NB-001 | 27/07/2026" : "SCHEDA ASSET";
        RefreshLogoImages();
        _previewFooter.Text = _showFooter.IsChecked == true ? Text(_footerText) : string.Empty;
        _previewHeader.Background = SafeBrush(Text(_primaryColor), "#0A84FF");
        var layout = NormalizeHeaderLayout(_headerLayout.SelectedItem?.ToString());
        _previewHeader.MinHeight = layout switch { "Enterprise" => 150, "Compatta" => 82, _ => 116 };
        _previewDetails.IsVisible = _showCompanyDetails.IsChecked == true;
        _previewCompany.FontSize = layout == "Compatta" ? 15 : 18;

        _previewSignatures.Children.Clear();
        if (_showSignatures.IsChecked == true)
        {
            _previewSignatures.Children.Add(SignaturePreview(Text(_leftSignature)));
            _previewSignatures.Children.Add(SignaturePreview(Text(_rightSignature)));
        }
    }

    private void RefreshLogoImages()
    {
        var show = _showLogo.IsChecked == true;
        var path = Text(_logoPath);
        Bitmap? bitmap = null;
        if (show && File.Exists(path))
        {
            try { bitmap = new Bitmap(path); }
            catch { bitmap = null; }
        }

        _companyLogoPreview.Source = bitmap;
        _companyLogoPreview.IsVisible = bitmap is not null;
        _companyLogoPlaceholder.IsVisible = bitmap is null;

        _previewLogoImage.Source = bitmap;
        _previewLogoImage.IsVisible = bitmap is not null;
        _previewLogoPlaceholder.IsVisible = show && bitmap is null;

        var size = _logoSize.SelectedItem?.ToString() ?? "Medio";
        var width = size switch { "Piccolo" => 110d, "Grande" => 180d, _ => 145d };
        _previewLogoImage.MaxWidth = width;
        _previewLogoImage.MaxHeight = 50;
        _previewLogoImage.HorizontalAlignment = (_logoPosition.SelectedItem?.ToString() ?? "Sinistra") switch
        {
            "Centro" => HorizontalAlignment.Center,
            "Destra" => HorizontalAlignment.Right,
            _ => HorizontalAlignment.Left
        };
    }

    private IReadOnlyList<string> BuildCompanyDetailLinesFromInputs()
    {
        var location = string.Join(" | ", new[]
        {
            Text(_address),
            string.Join(" ", new[] { Text(_city), Text(_province) }.Where(value => !string.IsNullOrWhiteSpace(value))),
            Text(_country)
        }.Where(value => !string.IsNullOrWhiteSpace(value)));
        var tax = string.Join(" | ", new[] { Prefix("P.IVA", Text(_vatNumber)), Prefix("C.F.", Text(_fiscalCode)) }
            .Where(value => !string.IsNullOrWhiteSpace(value)));
        var contacts = string.Join(" | ", new[] { Prefix("Tel.", Text(_phone)), Text(_email) }
            .Where(value => !string.IsNullOrWhiteSpace(value)));
        var digital = string.Join(" | ", new[] { Prefix("PEC", Text(_pec)), Text(_website) }
            .Where(value => !string.IsNullOrWhiteSpace(value)));
        var lines = new[] { location, tax, contacts, digital }.Where(value => !string.IsNullOrWhiteSpace(value)).ToList();
        return NormalizeHeaderLayout(_headerLayout.SelectedItem?.ToString()) == "Compatta" ? lines.Take(1).ToList() : lines;
    }

    private static IReadOnlyList<string> BuildCompanyDetailLines(CompanySettings company, string? layout)
    {
        var location = string.Join(" | ", new[]
        {
            company.Address,
            string.Join(" ", new[] { company.City, company.Province }.Where(value => !string.IsNullOrWhiteSpace(value))),
            company.Country
        }.Where(value => !string.IsNullOrWhiteSpace(value)));
        var tax = string.Join(" | ", new[] { Prefix("P.IVA", company.VatNumber), Prefix("C.F.", company.FiscalCode) }
            .Where(value => !string.IsNullOrWhiteSpace(value)));
        var contacts = string.Join(" | ", new[] { Prefix("Tel.", company.Phone), company.Email }
            .Where(value => !string.IsNullOrWhiteSpace(value)));
        var digital = string.Join(" | ", new[] { Prefix("PEC", company.Pec), company.Website }
            .Where(value => !string.IsNullOrWhiteSpace(value)));
        var lines = new[] { location, tax, contacts, digital }.Where(value => !string.IsNullOrWhiteSpace(value)).ToList();
        return NormalizeHeaderLayout(layout) == "Compatta" ? lines.Take(1).ToList() : lines;
    }

    private static string BuildCompanyDetails(CompanySettings company) =>
        string.Join(" | ", BuildCompanyDetailLines(company, "Corporate"));

    private static string Prefix(string label, string value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : $"{label} {value.Trim()}";

    private static string NormalizeHeaderLayout(string? value)
    {
        if (string.Equals(value, "Enterprise", StringComparison.OrdinalIgnoreCase)) return "Enterprise";
        if (string.Equals(value, "Compatta", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "Intestazione compatta", StringComparison.OrdinalIgnoreCase)) return "Compatta";
        return "Corporate";
    }

    private static Control PreviewSection(string title, string body) => new StackPanel
    {
        Spacing = 7,
        Children =
        {
            new TextBlock { Text = title, FontSize = 15, FontWeight = FontWeight.Bold },
            new Border
            {
                Background = UiTokens.Brush(UiTokens.SurfaceAlt),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(14),
                Child = new TextBlock { Text = body, LineHeight = 21 }
            }
        }
    };

    private static Control SignaturePreview(string label) => new StackPanel
    {
        Width = 250,
        Spacing = 18,
        Children =
        {
            new TextBlock { Text = string.IsNullOrWhiteSpace(label) ? "Firma" : label },
            new Border { BorderBrush = UiTokens.Brush(UiTokens.Border), BorderThickness = new Thickness(0, 0, 0, 1), Height = 1 }
        }
    };

    private static Control SectionTitle(string title, string subtitle) => new StackPanel
    {
        Spacing = 3,
        Children =
        {
            new TextBlock { Text = title, FontSize = 21, FontWeight = FontWeight.Bold },
            new TextBlock { Text = subtitle, Foreground = UiTokens.Brush(UiTokens.TextSecondary), TextWrapping = TextWrapping.Wrap }
        }
    };

    private static Control Field(string label, Control input) => new StackPanel
    {
        Margin = new Thickness(4),
        Spacing = 4,
        Children =
        {
            new TextBlock { Text = label, FontSize = 12, FontWeight = FontWeight.SemiBold, Foreground = UiTokens.Brush(UiTokens.TextSecondary) },
            input
        }
    };

    private static Border Card(Control child) => new()
    {
        Background = Brushes.White,
        BorderBrush = UiTokens.Brush(UiTokens.Border),
        BorderThickness = new Thickness(1),
        CornerRadius = new CornerRadius(14),
        Padding = new Thickness(16),
        Child = child
    };

    private static Control InfoCard(string title, string text) => Card(new StackPanel
    {
        Spacing = 5,
        Children =
        {
            new TextBlock { Text = title, FontWeight = FontWeight.Bold },
            new TextBlock { Text = text, Foreground = UiTokens.Brush(UiTokens.TextSecondary), TextWrapping = TextWrapping.Wrap }
        }
    });

    private static ScrollViewer Scroll(Control content) => new()
    {
        Content = content,
        VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
        HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled
    };

    private static Button Button(string text, Action action, bool primary)
    {
        var button = new Button
        {
            Content = text,
            Margin = new Thickness(5),
            Padding = new Thickness(13, 9),
            CornerRadius = new CornerRadius(9),
            HorizontalContentAlignment = HorizontalAlignment.Center,
            Background = primary ? UiTokens.Brush(UiTokens.BrandBlue) : UiTokens.Brush(UiTokens.SurfaceAlt),
            Foreground = primary ? Brushes.White : UiTokens.Brush(UiTokens.TextPrimary),
            FontWeight = FontWeight.SemiBold
        };
        button.Click += (_, _) => action();
        return button;
    }

    private void ShowMessage(string message, bool error = false)
    {
        _message.Text = message;
        _message.Foreground = UiTokens.Brush(error ? UiTokens.Danger : UiTokens.BrandBlue);
    }

    private static string Text(TextBox box) => (box.Text ?? string.Empty).Trim();
    private static string NormalizeColor(string value, string fallback) =>
        value.StartsWith('#') && value.Length == 7 ? value.ToUpperInvariant() : fallback;

    private static IBrush SafeBrush(string value, string fallback)
    {
        try { return Brush.Parse(NormalizeColor(value, fallback)); }
        catch { return Brush.Parse(fallback); }
    }

    private static void Add(Grid grid, Control control, int column, int row)
    {
        Grid.SetColumn(control, column);
        Grid.SetRow(control, row);
        grid.Children.Add(control);
    }
}
