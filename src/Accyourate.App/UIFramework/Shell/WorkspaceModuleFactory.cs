using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Accyourate.App.Data;
using Accyourate.App.DigitalTwin;
using Accyourate.App.Models;
using Accyourate.App.UIFramework.Components;
using Accyourate.App.UIFramework.Tokens;
using Accyourate.App.UIFramework.Widgets;
using Accyourate.App.Platform.Notifications;

namespace Accyourate.App.UIFramework.Shell;

public sealed class WorkspaceModuleFactory
{
    private readonly DatabaseService _database;
    private readonly CurrentUser _user;
    private readonly Action<string, string>? _navigate;

    public WorkspaceModuleFactory(DatabaseService database, CurrentUser user)
        : this(database, user, null)
    {
    }

    public WorkspaceModuleFactory(DatabaseService database, CurrentUser user, Action<string, string>? navigate)
    {
        _database = database;
        _user = user;
        _navigate = navigate;
    }

    public Control Create(string moduleId)
    {
        return moduleId switch
        {
            "workspace-home" => WorkspaceHome(),
            "control-room" => new WidgetControlRoomBuilder(_database, _user).Build(() => { }),
            "ai-catalog" => IntentCatalogSummary(),
            "notifications" => new NotificationCenterView(),
            "dashboard" => DashboardWorkspace(),
            "analytics" => AnalyticsWorkspace(),
            "medical" => MedicalWorkspace(),
            "digital-twin" => new DigitalTwinWorkspaceModule(_database).Build(),
            "branding" => BrandingSummary(),
            "design-system" => DesignSystemSummary(),
            "architecture" => ArchitectureSummary(),
            _ => Placeholder(moduleId)
        };
    }

    private Control IntentCatalogSummary()
    {
        return SummaryPage("AI Intent Catalog", "Catalogo intenti AI integrato nella Workspace.", new[]
        {
            "Intenti disponibili",
            "Azioni collegate",
            "Routing verso Action Engine",
            "Base futura per comandi AI contestuali"
        });
    }


    private Control WorkspaceHome()
    {
        var page = Page($"Benvenuto, {_user.Username}", "Enterprise Home · la tua centrale operativa quotidiana.");

        var welcome = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,320"),
            Margin = new Thickness(0, 0, 0, 4)
        };

        Add(welcome, WelcomePanel(), 0, 0);
        Add(welcome, UiComponentFactory.Card(SystemStatusHome()), 1, 0);
        page.Children.Add(welcome);

        var kpis = new WrapPanel { ItemWidth = 230, ItemHeight = 132 };
        kpis.Children.Add(Kpi("👥", "Dipendenti", Count("employees"), "Anagrafica", UiTokens.BrandBlue));
        kpis.Children.Add(Kpi("▣", "Asset IT", Count("assets"), "Inventario", UiTokens.Success));
        kpis.Children.Add(Kpi("🏢", "Aziende", Count("companies"), "Master Data", UiTokens.BrandAccent));
        kpis.Children.Add(Kpi("▧", "Documenti", Count("documents"), "Archivio", UiTokens.Warning));
        kpis.Children.Add(Kpi("✓", "Attività", Count("workflow_events"), "Timeline", UiTokens.Info));
        page.Children.Add(kpis);

        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,*") };
        Add(grid, UiComponentFactory.Card(QuickActionsHome()), 0, 0);
        Add(grid, UiComponentFactory.Card(HomeNotifications()), 1, 0);
        page.Children.Add(grid);

        var lower = new Grid { ColumnDefinitions = new ColumnDefinitions("*,*") };
        Add(lower, UiComponentFactory.Card(FavoriteModulesHome()), 0, 0);
        Add(lower, UiComponentFactory.Card(TodayActivityHome()), 1, 0);
        page.Children.Add(lower);

        var final = new Grid { ColumnDefinitions = new ColumnDefinitions("*,*") };
        Add(final, UiComponentFactory.Card(RecentEvents()), 0, 0);
        Add(final, UiComponentFactory.Card(AiWelcomeHome()), 1, 0);
        page.Children.Add(final);

        return Scroll(page);
    }

    private Control WelcomePanel()
    {
        var stack = new StackPanel { Spacing = 10 };

        stack.Children.Add(new TextBlock
        {
            Text = $"Buongiorno {_user.Username}",
            FontSize = 30,
            FontWeight = FontWeight.Bold,
            Foreground = UiTokens.Brush(UiTokens.TextPrimary)
        });

        stack.Children.Add(new TextBlock
        {
            Text = $"{DateTime.Now:dddd dd MMMM yyyy} · {DateTime.Now:HH:mm}",
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            TextWrapping = TextWrapping.Wrap
        });

        stack.Children.Add(new TextBlock
        {
            Text = $"Hai {SafeCount("assets")} asset censiti, {SafeCount("employees")} dipendenti e {SafeCount("workflow_events")} eventi registrati. Usa gli accessi rapidi per continuare il lavoro.",
            Foreground = UiTokens.Brush(UiTokens.TextSecondary),
            TextWrapping = TextWrapping.Wrap
        });

        return new Border
        {
            Background = UiTokens.Brush(UiTokens.Surface),
            CornerRadius = new CornerRadius(18),
            Padding = new Thickness(18),
            Child = stack
        };
    }

    private Control QuickActionsHome()
    {
        var stack = new StackPanel { Spacing = 10 };
        stack.Children.Add(SectionTitle("Accessi rapidi"));
        stack.Children.Add(QuickAction("▣", "Asset Management", "Inventario, assegnazioni e garanzie", "asset-management", "Asset Management"));
        stack.Children.Add(QuickAction("🏢", "Anagrafica Aziendale", "Dipendenti, sedi, reparti e fornitori", "master-data", "Anagrafica Aziendale"));
        stack.Children.Add(QuickAction("AI", "AI Assistant", "Supporto operativo e comandi intelligenti", "ai-assistant", "AI Assistant"));
        stack.Children.Add(QuickAction("◈", "Branding Center", "Logo, login e identità aziendale", "branding", "Branding Center"));
        return stack;
    }


    private Control FavoriteModulesHome()
    {
        var stack = new StackPanel { Spacing = 10 };
        stack.Children.Add(SectionTitle("I miei preferiti"));
        stack.Children.Add(QuickAction("★", "Asset Management", "Modulo operativo principale", "asset-management", "Asset Management"));
        stack.Children.Add(QuickAction("★", "Anagrafica Aziendale", "Master Data aziendale", "master-data", "Anagrafica Aziendale"));
        stack.Children.Add(QuickAction("★", "Dashboard", "KPI e sintesi operative", "dashboard", "Dashboard"));
        return stack;
    }

    private Control TodayActivityHome()
    {
        var stack = new StackPanel { Spacing = 10 };
        stack.Children.Add(SectionTitle("Attività di oggi"));
        stack.Children.Add(StatusLine("Asset censiti", Count("assets"), UiTokens.BrandBlue));
        stack.Children.Add(StatusLine("Dipendenti", Count("employees"), UiTokens.Success));
        stack.Children.Add(StatusLine("Eventi workflow", Count("workflow_events"), UiTokens.Info));
        stack.Children.Add(StatusLine("Documenti", Count("documents"), UiTokens.Warning));
        return stack;
    }

    private Control HomeNotifications()
    {
        var stack = new StackPanel { Spacing = 10 };
        stack.Children.Add(SectionTitle("Notifiche"));
        stack.Children.Add(StatusLine("Garanzie", "Da verificare", UiTokens.Warning));
        stack.Children.Add(StatusLine("Asset assegnati", Count("AssetAssignments"), UiTokens.BrandBlue));
        stack.Children.Add(StatusLine("Database", "Operativo", UiTokens.Success));
        stack.Children.Add(StatusLine("Backup", "Pianificare", UiTokens.Warning));
        return stack;
    }

    private Control SystemStatusHome()
    {
        var stack = new StackPanel { Spacing = 10 };
        stack.Children.Add(SectionTitle("Stato sistema"));
        stack.Children.Add(StatusLine("Versione", "15.0.1A", UiTokens.BrandBlue));
        stack.Children.Add(StatusLine("Database", "Connesso", UiTokens.Success));
        stack.Children.Add(StatusLine("Workspace", "Tab attive", UiTokens.Success));
        stack.Children.Add(StatusLine("Login", "Brandizzato", UiTokens.BrandAccent));
        return stack;
    }

    private Control AiWelcomeHome()
    {
        var stack = new StackPanel { Spacing = 10 };
        stack.Children.Add(SectionTitle("AI Welcome"));
        stack.Children.Add(UiComponentFactory.Body("L'assistente AI diventerà il punto unico per cercare dati, aprire moduli e avviare azioni operative."));
        stack.Children.Add(UiComponentFactory.Body("Prossimo step: suggerimenti intelligenti su asset, dipendenti, scadenze e notifiche."));
        return stack;
    }

    private Control DashboardWorkspace()
    {
        var page = Page("Dashboard operativa", "KPI e sintesi operative direttamente dentro la Workspace.");

        var kpis = new WrapPanel { ItemWidth = 250, ItemHeight = 132 };
        kpis.Children.Add(Kpi("👥", "Persone", Count("employees"), "Anagrafiche", UiTokens.BrandBlue));
        kpis.Children.Add(Kpi("▣", "Asset IT", Count("assets"), "Dispositivi", UiTokens.Success));
        kpis.Children.Add(Kpi("⌁", "Medical", Count("medical_devices"), "Dispositivi medici", UiTokens.BrandAccent));
        kpis.Children.Add(Kpi("▧", "Documenti", Count("documents"), "Archivio", UiTokens.Warning));
        kpis.Children.Add(Kpi("✓", "Qualità", Count("quality_tests"), "Test", UiTokens.Success));
        kpis.Children.Add(Kpi("⚙", "Manutenzioni", Count("maintenance_records"), "Interventi", UiTokens.Danger));
        page.Children.Add(kpis);

        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,*") };
        Add(grid, UiComponentFactory.Card(RecentEvents()), 0, 0);
        Add(grid, UiComponentFactory.Card(SystemOverview()), 1, 0);
        page.Children.Add(grid);

        var quick = new WrapPanel { ItemWidth = 240, ItemHeight = 76 };
        quick.Children.Add(QuickCard("Medical Device Suite", "Gestione dispositivi"));
        quick.Children.Add(QuickCard("Document Management", "Archivio documentale"));
        quick.Children.Add(QuickCard("Enterprise Architecture", "Health report"));
        quick.Children.Add(QuickCard("Branding Center", "Identità aziendale"));
        page.Children.Add(UiComponentFactory.Card(new StackPanel
        {
            Spacing = 12,
            Children =
            {
                SectionTitle("Accessi rapidi"),
                quick
            }
        }));

        return Scroll(page);
    }

    private Control AnalyticsWorkspace()
    {
        var page = Page("Analytics", "Prime analisi operative integrate direttamente nella Workspace.");

        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,*") };
        Add(grid, UiComponentFactory.Card(OperationalVolumeChart()), 0, 0);
        Add(grid, UiComponentFactory.Card(StatusSummary()), 1, 0);
        page.Children.Add(grid);

        var second = new Grid { ColumnDefinitions = new ColumnDefinitions("*,*") };
        Add(second, UiComponentFactory.Card(TrendPlaceholder()), 0, 0);
        Add(second, UiComponentFactory.Card(AnalyticsNotes()), 1, 0);
        page.Children.Add(second);

        return Scroll(page);
    }

    private Control RecentEvents()
    {
        var stack = new StackPanel { Spacing = 10 };
        stack.Children.Add(SectionTitle("Ultimi eventi"));
        try
        {
            var events = _database.GetWorkflowEvents(null, null, 8);
            if (events.Count == 0)
                stack.Children.Add(UiComponentFactory.Body("Nessun evento disponibile."));

            foreach (var ev in events)
                stack.Children.Add(UiComponentFactory.Body($"{ev.CreatedAt} · {ev.EntityType} {ev.EntityCode} · {ev.EventType}"));
        }
        catch
        {
            stack.Children.Add(UiComponentFactory.Body("Eventi non disponibili in questa installazione."));
        }
        return stack;
    }

    private Control SystemOverview()
    {
        var stack = new StackPanel { Spacing = 10 };
        stack.Children.Add(SectionTitle("Stato sistemi"));
        stack.Children.Add(StatusLine("Database", "Connesso", UiTokens.Success));
        stack.Children.Add(StatusLine("Medical Suite", "Attiva", UiTokens.Success));
        stack.Children.Add(StatusLine("Document Management", "Attivo", UiTokens.Success));
        stack.Children.Add(StatusLine("Analytics", "Workspace", UiTokens.BrandBlue));
        stack.Children.Add(StatusLine("Backup", "Da verificare", UiTokens.Warning));
        return stack;
    }

    private Control OperationalVolumeChart()
    {
        var stack = new StackPanel { Spacing = 10 };
        stack.Children.Add(SectionTitle("Volumi operativi"));

        var data = new[]
        {
            ("Produzione", SafeCount("production_orders")),
            ("Qualità", SafeCount("quality_tests")),
            ("Magazzino", SafeCount("stock_movements")),
            ("Lavaggi", SafeCount("laundry_cycles")),
            ("Manutenzioni", SafeCount("maintenance_records")),
            ("Documenti", SafeCount("documents"))
        };

        var max = Math.Max(1, data.Max(x => x.Item2));

        foreach (var item in data)
        {
            var row = new Grid { ColumnDefinitions = new ColumnDefinitions("120,*,60") };
            Add(row, new TextBlock { Text = item.Item1, Foreground = UiTokens.Brush(UiTokens.TextSecondary) }, 0, 0);
            Add(row, new Border
            {
                Width = Math.Max(8, item.Item2 * 320.0 / max),
                Height = 14,
                Background = UiTokens.Brush(UiTokens.BrandBlue),
                CornerRadius = new CornerRadius(7),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left
            }, 1, 0);
            Add(row, new TextBlock { Text = item.Item2.ToString(), FontWeight = FontWeight.Bold, Foreground = UiTokens.Brush(UiTokens.TextPrimary) }, 2, 0);
            stack.Children.Add(row);
        }

        return stack;
    }

    private Control StatusSummary()
    {
        var stack = new StackPanel { Spacing = 10 };
        stack.Children.Add(SectionTitle("Sintesi stato"));
        stack.Children.Add(StatusLine("Dispositivi medici", Count("medical_devices"), UiTokens.BrandBlue));
        stack.Children.Add(StatusLine("Asset IT", Count("assets"), UiTokens.Success));
        stack.Children.Add(StatusLine("Documenti", Count("documents"), UiTokens.Warning));
        stack.Children.Add(StatusLine("Audit records", Count("audit_logs"), UiTokens.BrandAccent));
        return stack;
    }

    private Control TrendPlaceholder()
    {
        var stack = new StackPanel { Spacing = 10 };
        stack.Children.Add(SectionTitle("Trend settimanale"));
        var values = new[] { 18, 26, 21, 38, 44, 33, 47 };
        var labels = new[] { "Lun", "Mar", "Mer", "Gio", "Ven", "Sab", "Dom" };

        for (var i = 0; i < values.Length; i++)
        {
            var row = new Grid { ColumnDefinitions = new ColumnDefinitions("60,*,50") };
            Add(row, new TextBlock { Text = labels[i], Foreground = UiTokens.Brush(UiTokens.TextSecondary) }, 0, 0);
            Add(row, new Border
            {
                Height = 12,
                Width = values[i] * 8,
                Background = UiTokens.Brush(UiTokens.Success),
                CornerRadius = new CornerRadius(6),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left
            }, 1, 0);
            Add(row, new TextBlock { Text = values[i].ToString(), FontWeight = FontWeight.Bold, Foreground = UiTokens.Brush(UiTokens.TextPrimary) }, 2, 0);
            stack.Children.Add(row);
        }

        return stack;
    }

    private Control AnalyticsNotes()
    {
        var stack = new StackPanel { Spacing = 10 };
        stack.Children.Add(SectionTitle("Prossima evoluzione"));
        stack.Children.Add(UiComponentFactory.Body("8.2.1: filtri data e reparto."));
        stack.Children.Add(UiComponentFactory.Body("8.2.2: grafici più evoluti."));
        stack.Children.Add(UiComponentFactory.Body("8.3: migrazione Medical Suite nella Workspace."));
        stack.Children.Add(UiComponentFactory.Body("8.4: report PDF/Excel."));
        return stack;
    }


    private Control MedicalWorkspace()
    {
        var page = Page("Medical Device Suite", "Modulo Medical integrato nella Workspace: dispositivi, qualità, produzione, manutenzioni e Digital Twin in un'unica vista.");

        var kpis = new WrapPanel { ItemWidth = 250, ItemHeight = 132 };
        kpis.Children.Add(Kpi("⌁", "Dispositivi", Count("medical_devices"), "Anagrafica medicale", UiTokens.BrandBlue));
        kpis.Children.Add(Kpi("▤", "Produzione", Count("production_orders"), "Ordini", UiTokens.BrandAccent));
        kpis.Children.Add(Kpi("✓", "Qualità", Count("quality_tests"), "Test qualità", UiTokens.Success));
        kpis.Children.Add(Kpi("⚙", "Manutenzioni", Count("maintenance_records"), "Interventi", UiTokens.Warning));
        kpis.Children.Add(Kpi("▧", "Lavaggi", Count("laundry_cycles"), "Cicli", UiTokens.Info));
        kpis.Children.Add(Kpi("◇", "Digital Twin", Count("workflow_events"), "Eventi", UiTokens.Danger));
        page.Children.Add(kpis);

        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,*") };
        Add(grid, UiComponentFactory.Card(MedicalLifecycle()), 0, 0);
        Add(grid, UiComponentFactory.Card(MedicalStatus()), 1, 0);
        page.Children.Add(grid);

        var lower = new Grid { ColumnDefinitions = new ColumnDefinitions("*,*") };
        Add(lower, UiComponentFactory.Card(MedicalRecentEvents()), 0, 0);
        Add(lower, UiComponentFactory.Card(MedicalQuickActions()), 1, 0);
        page.Children.Add(lower);

        return Scroll(page);
    }

    private Control MedicalLifecycle()
    {
        var stack = new StackPanel { Spacing = 12 };
        stack.Children.Add(SectionTitle("Lifecycle dispositivo"));

        stack.Children.Add(LifecycleStep("1", "Registrazione", "Creazione anagrafica dispositivo", UiTokens.BrandBlue));
        stack.Children.Add(LifecycleStep("2", "Produzione", "Ordini e avanzamento produzione", UiTokens.BrandAccent));
        stack.Children.Add(LifecycleStep("3", "Qualità", "Test, controlli e conformità", UiTokens.Success));
        stack.Children.Add(LifecycleStep("4", "Logistica", "Magazzino, spedizione e rientro", UiTokens.Warning));
        stack.Children.Add(LifecycleStep("5", "Assistenza", "Lavaggi, manutenzioni e riparazioni", UiTokens.Danger));
        stack.Children.Add(LifecycleStep("6", "Digital Twin", "Storico completo eventi e stati", UiTokens.Info));

        return stack;
    }

    private Control MedicalStatus()
    {
        var stack = new StackPanel { Spacing = 10 };
        stack.Children.Add(SectionTitle("Stato operativo Medical Suite"));
        stack.Children.Add(StatusLine("Dispositivi medici", Count("medical_devices"), UiTokens.BrandBlue));
        stack.Children.Add(StatusLine("Ordini produzione", Count("production_orders"), UiTokens.BrandAccent));
        stack.Children.Add(StatusLine("Test qualità", Count("quality_tests"), UiTokens.Success));
        stack.Children.Add(StatusLine("Movimenti magazzino", Count("stock_movements"), UiTokens.Warning));
        stack.Children.Add(StatusLine("Cicli lavaggio", Count("laundry_cycles"), UiTokens.Info));
        stack.Children.Add(StatusLine("Interventi manutenzione", Count("maintenance_records"), UiTokens.Danger));
        return stack;
    }

    private Control MedicalRecentEvents()
    {
        var stack = new StackPanel { Spacing = 10 };
        stack.Children.Add(SectionTitle("Eventi Digital Twin recenti"));

        try
        {
            var events = _database.GetWorkflowEvents(null, null, 10);
            if (events.Count == 0)
            {
                stack.Children.Add(UiComponentFactory.Body("Nessun evento Digital Twin disponibile."));
                return stack;
            }

            foreach (var ev in events)
            {
                stack.Children.Add(UiComponentFactory.Body($"{ev.CreatedAt} · {ev.EntityType} {ev.EntityCode} · {ev.EventType} → {ev.ToStatus}"));
            }
        }
        catch
        {
            stack.Children.Add(UiComponentFactory.Body("Eventi Digital Twin non disponibili in questa installazione."));
        }

        return stack;
    }

    private Control MedicalQuickActions()
    {
        var stack = new StackPanel { Spacing = 10 };
        stack.Children.Add(SectionTitle("Azioni Medical"));

        stack.Children.Add(UiComponentFactory.Body("• Apri la vecchia Medical Device Suite come fallback."));
        stack.Children.Add(UiComponentFactory.Body("• Consulta produzione, qualità, logistica e manutenzioni dalle finestre validate."));
        stack.Children.Add(UiComponentFactory.Body("• Nella prossima release queste azioni saranno convertite in pulsanti interni alla Workspace."));

        var note = new Border
        {
            Background = UiTokens.Brush(UiTokens.SurfaceAlt),
            CornerRadius = new Avalonia.CornerRadius(12),
            Padding = new Avalonia.Thickness(12),
            Child = UiComponentFactory.Body("Questa è la prima migrazione della Medical Suite nella Workspace: vista riepilogativa stabile, senza perdere le funzionalità validate.")
        };

        stack.Children.Add(note);
        return stack;
    }

    private Control LifecycleStep(string number, string title, string subtitle, string color)
    {
        var row = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Spacing = 12
        };

        row.Children.Add(new Border
        {
            Width = 34,
            Height = 34,
            Background = UiTokens.Brush(color),
            CornerRadius = new Avalonia.CornerRadius(17),
            Child = new TextBlock
            {
                Text = number,
                Foreground = Brushes.White,
                FontWeight = FontWeight.Bold,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            }
        });

        var text = new StackPanel();
        text.Children.Add(new TextBlock { Text = title, FontWeight = FontWeight.Bold, Foreground = UiTokens.Brush(UiTokens.TextPrimary) });
        text.Children.Add(new TextBlock { Text = subtitle, Foreground = UiTokens.Brush(UiTokens.TextSecondary), FontSize = 12 });
        row.Children.Add(text);

        return row;
    }


    private Control BrandingSummary() => SummaryPage("Branding Center", "Il branding diventerà un modulo interno della shell.", new[] { "Logo", "Hero image", "Nome azienda", "Tema" });
    private Control DesignSystemSummary() => SummaryPage("Design System", "Componenti comuni usati per costruire tutte le schermate future.", new[] { "Tokens", "Cards", "Buttons", "Badges", "Layout" });
    private Control ArchitectureSummary() => SummaryPage("Enterprise Architecture", "Fondazione tecnica e piano migrazioni.", new[] { "Health report", "API foundation", "Database migrations", "Feature flags" });
    private Control Placeholder(string moduleId) => SummaryPage("Modulo in migrazione", $"Il modulo '{moduleId}' è registrato nella shell e sarà migrato progressivamente.", new[] { "Funzionalità esistente preservata", "Migrazione UI futura", "Test incrementale" });

    private Control SummaryPage(string title, string subtitle, IEnumerable<string> bullets)
    {
        var page = Page(title, subtitle);
        var list = new StackPanel { Spacing = 10 };
        foreach (var b in bullets)
            list.Children.Add(UiComponentFactory.Body($"• {b}"));
        page.Children.Add(UiComponentFactory.Card(list));
        return Scroll(page);
    }

    private StackPanel Page(string title, string subtitle)
    {
        var page = new StackPanel { Margin = new Thickness(24), Spacing = 18 };
        page.Children.Add(UiComponentFactory.Title(title));
        page.Children.Add(UiComponentFactory.Body(subtitle));
        return page;
    }

    private ScrollViewer Scroll(Control content) => new()
    {
        Content = content,
        VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
    };

    private Border Kpi(string icon, string title, string value, string subtitle, string color)
    {
        var row = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 14 };
        row.Children.Add(new Border
        {
            Width = 52,
            Height = 52,
            Background = UiTokens.Brush(color),
            CornerRadius = new CornerRadius(14),
            Child = new TextBlock
            {
                Text = icon,
                FontSize = 23,
                Foreground = Brushes.White,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            }
        });

        var col = new StackPanel();
        col.Children.Add(new TextBlock { Text = title, Foreground = UiTokens.Brush(UiTokens.TextPrimary) });
        col.Children.Add(new TextBlock { Text = value, FontSize = 26, FontWeight = FontWeight.Bold, Foreground = UiTokens.Brush(UiTokens.TextPrimary) });
        col.Children.Add(new TextBlock { Text = subtitle, Foreground = UiTokens.Brush(UiTokens.TextSecondary) });
        row.Children.Add(col);

        return UiComponentFactory.Card(row);
    }


    private Control QuickAction(string icon, string title, string subtitle, string moduleId, string moduleTitle)
    {
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("42,*") };

        Add(grid, new Border
        {
            Width = 34,
            Height = 34,
            Background = UiTokens.Brush(UiTokens.SurfaceAlt),
            CornerRadius = new CornerRadius(10),
            Child = new TextBlock
            {
                Text = icon,
                FontWeight = FontWeight.Bold,
                Foreground = UiTokens.Brush(UiTokens.BrandBlue),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            }
        }, 0, 0);

        var text = new StackPanel { Spacing = 2 };
        text.Children.Add(new TextBlock { Text = title, FontWeight = FontWeight.Bold, Foreground = UiTokens.Brush(UiTokens.TextPrimary) });
        text.Children.Add(new TextBlock { Text = subtitle, Foreground = UiTokens.Brush(UiTokens.TextSecondary), FontSize = 12, TextWrapping = TextWrapping.Wrap });
        Add(grid, text, 1, 0);

        var button = new Button
        {
            Content = grid,
            Background = Brushes.Transparent,
            Padding = new Thickness(8),
            CornerRadius = new CornerRadius(12),
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
        };

        button.Click += (_, _) => _navigate?.Invoke(moduleId, moduleTitle);
        return button;
    }

    private Control QuickCard(string title, string subtitle)
    {
        var stack = new StackPanel { Spacing = 4 };
        stack.Children.Add(new TextBlock { Text = title, FontWeight = FontWeight.Bold, Foreground = UiTokens.Brush(UiTokens.TextPrimary) });
        stack.Children.Add(new TextBlock { Text = subtitle, Foreground = UiTokens.Brush(UiTokens.TextSecondary), FontSize = 12 });
        return stack;
    }

    private Control StatusLine(string label, string value, string color)
    {
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,120") };
        Add(grid, new TextBlock { Text = label, Foreground = UiTokens.Brush(UiTokens.TextSecondary) }, 0, 0);
        Add(grid, new TextBlock { Text = value, FontWeight = FontWeight.Bold, Foreground = UiTokens.Brush(color), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right }, 1, 0);
        return grid;
    }

    private string Count(string table) => SafeCount(table).ToString();

    private int SafeCount(string table)
    {
        try { return _database.CountTable(table); }
        catch { return 0; }
    }

    private static TextBlock SectionTitle(string text) => new()
    {
        Text = text,
        FontSize = 18,
        FontWeight = FontWeight.Bold,
        Foreground = UiTokens.Brush(UiTokens.TextPrimary)
    };

    private static void Add(Grid grid, Control control, int column, int row)
    {
        Grid.SetColumn(control, column);
        Grid.SetRow(control, row);
        grid.Children.Add(control);
    }
}
