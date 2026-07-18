using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.UIFramework.Tokens;
using Accyourate.App.UIFramework.DesignSystem;

namespace Accyourate.App.Platform.Dashboard;

public sealed class EnterpriseDashboardView : UserControl
{
    private readonly DashboardKpiService _service = new();
    private readonly WrapPanel _kpis = new();
    private readonly StackPanel _actions = new();
    private readonly TextBlock _message = new();

    private DashboardSnapshot _snapshot = new();

    public EnterpriseDashboardView()
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
            Margin = new Thickness(24, 20, 24, 16),
            ColumnDefinitions = new ColumnDefinitions("*,130")
        };

        var title = new StackPanel { Spacing = 6 };
        title.Children.Add(new TextBlock
        {
            Text = "Enterprise Dashboard",
            FontSize = 34,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });

        title.Children.Add(new TextBlock
        {
            Text = "Cruscotto operativo con KPI di piattaforma, HR, Asset, Verbali e Documenti.",
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            TextWrapping = TextWrapping.Wrap
        });

        Add(header, title, 0, 0);
        Add(header, ToolbarButton("↻ Aggiorna", Load), 1, 0);

        DockPanel.SetDock(header, Dock.Top);
        root.Children.Add(header);

        _message.Margin = new Thickness(24, 0, 24, 12);
        _message.Foreground = UiTokens.Brush(UiTokens.BrandBlue);
        DockPanel.SetDock(_message, Dock.Top);
        root.Children.Add(_message);

        var content = new StackPanel
        {
            Margin = new Thickness(24, 0, 24, 24),
            Spacing = 18
        };

        _kpis.ItemWidth = 252;
        _kpis.ItemHeight = 202;
        content.Children.Add(_kpis);

        content.Children.Add(Section("Azioni rapide", _actions));

        var notes = new StackPanel { Spacing = 8 };
        notes.Children.Add(Info("Beta 0.9", "Questa Dashboard è la base del cruscotto operativo. Nei prossimi sprint verranno aggiunti trend, attività recenti e link contestuali."));
        notes.Children.Add(Info("Origine dati", "I KPI vengono letti dai database HR, Asset e Platform. Se un database o una tabella non esiste ancora, il valore viene mostrato come 0."));
        content.Children.Add(Section("Stato piattaforma", notes));

        root.Children.Add(new ScrollViewer
        {
            Content = content,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        });

        return root;
    }

    private void Load()
    {
        _snapshot = _service.Load();
        RefreshKpis();
        RefreshActions();

        _message.Text = $"Dashboard aggiornata alle {FormatDate(_snapshot.LastRefresh)}.";
    }

    private void RefreshKpis()
    {
        _kpis.Children.Clear();

        _kpis.Children.Add(Kpi("👥", _snapshot.Employees.ToString(), "Dipendenti", $"{_snapshot.ActiveEmployees} attivi"));
        _kpis.Children.Add(Kpi("💻", _snapshot.Assets.ToString(), "Asset", $"{_snapshot.AssignedAssets} assegnati"));
        _kpis.Children.Add(Kpi("📄", _snapshot.DeliveryReports.ToString(), "Verbali", $"{_snapshot.GeneratedDeliveryReports} PDF generati"));
        _kpis.Children.Add(Kpi("📁", _snapshot.Documents.ToString(), "Documenti", "Archivio documentale"));
        _kpis.Children.Add(Kpi("🔔", _snapshot.UnreadNotifications.ToString(), "Notifiche", "Da leggere"));
        _kpis.Children.Add(Kpi("📜", _snapshot.AuditEvents.ToString(), "Audit", "Eventi registrati"));
    }

    private void RefreshActions()
    {
        _actions.Children.Clear();
        _actions.Spacing = 10;

        _actions.Children.Add(Info("Human Resources", "Gestisci dipendenti e profili."));
        _actions.Children.Add(Info("Asset Management", "Inventario, assegnazioni e restituzioni."));
        _actions.Children.Add(Info("Verbali consegna", "Genera e consulta i verbali PDF."));
        _actions.Children.Add(Info("Centro Documenti", "Apri e cerca documenti generati dalla piattaforma."));
        _actions.Children.Add(Info("Impostazioni", "Configura azienda, numerazioni e percorsi."));
    }

    private static Control Kpi(string icon, string value, string label, string subtitle)
    {
        return AxKpiCard.Create(icon, label, value, subtitle);
    }

    private static Control Section(string title, Control content)
    {
        var stack = new StackPanel { Spacing = 10 };

        stack.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 22,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });

        stack.Children.Add(content);

        return new Border
        {
            Background = UiTokens.Brush(UiTokens.Surface),
            CornerRadius = new CornerRadius(22),
            Padding = new Thickness(18),
            Child = stack
        };
    }

    private static Border Info(string label, string value)
    {
        return new Border
        {
            Background = UiTokens.Brush(UiTokens.SurfaceAlt),
            CornerRadius = new CornerRadius(14),
            Padding = new Thickness(14),
            Child = new StackPanel
            {
                Spacing = 4,
                Children =
                {
                    new TextBlock
                    {
                        Text = label,
                        FontWeight = FontWeight.Bold,
                        Foreground = UiTokens.Brush(UiTokens.TextPrimary)
                    },
                    new TextBlock
                    {
                        Text = value,
                        Foreground = UiTokens.Brush(UiTokens.TextSecondary),
                        TextWrapping = TextWrapping.Wrap
                    }
                }
            }
        };
    }

    private static Button ToolbarButton(string text, Action action)
    {
        var b = new Button
        {
            Content = text,
            Background = UiTokens.Brush(UiTokens.BrandBlue),
            Foreground = Brushes.White,
            FontWeight = FontWeight.Bold,
            Padding = new Thickness(12, 8),
            CornerRadius = new CornerRadius(12)
        };

        b.Click += (_, _) => action();
        return b;
    }

    private static string FormatDate(string value)
    {
        return DateTime.TryParse(value, out var date)
            ? date.ToString("dd/MM/yyyy HH:mm")
            : value;
    }

    private static void Add(Grid grid, Control control, int column, int row)
    {
        Grid.SetColumn(control, column);
        Grid.SetRow(control, row);
        grid.Children.Add(control);
    }
}
