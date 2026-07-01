# Changelog

## 11.1.0 RC2.2.1 Ambiguous References Hotfix

- Risolti riferimenti ambigui per `IWorkspaceModule` e `WorkspaceModuleRegistry`.
- Aggiunti alias espliciti in `EnterpriseWorkspaceWindow.cs`.


## 11.1.0 RC2.2 Dashboard Module Registration

- Aggiunto `DashboardWorkspaceModule`.
- Dashboard registrata nel `WorkspaceModuleRegistry`.
- Apertura Dashboard tramite registry con fallback al comportamento precedente.
- Nessun cambiamento visivo previsto.


## 11.1.0 RC2.1 OpenModule API Foundation

- Aggiunto `IWorkspaceModule`.
- Aggiunto `WorkspaceModule`.
- Aggiunto `WorkspaceModuleDescriptor`.
- Aggiunto `WorkspaceModuleRegistry`.
- Nessun cambiamento visibile alla Workspace.


## 11.0.5 AI Assistant Tab

- Aggiunto EnterpriseAiAssistantView.
- EnterpriseAiAssistantWindow trasformato in wrapper della view.
- AI Assistant migrato come scheda interna della Workspace.
- Nessuna modifica ad Action Engine e Universal Command Bar.


## 11.0.4 Workspace Stabilization

- Consolidato WorkspaceTabManager centrale.
- Dashboard e Digital Twin ora condividono lo stesso WorkspaceHost.
- Aggiunto WorkspaceState foundation.
- Aggiunto snapshot tab aperte.
- Aggiunta gestione errore modulo tramite tab dedicata.


## 11.0.3 Digital Twin Tab

- Digital Twin migrato come scheda interna della Workspace.
- Aggiunto uso reale di WorkspaceHost e WorkspaceTabManager per Digital Twin.
- Scheda Digital Twin chiudibile.
- Nessuna modifica agli altri moduli principali.


## 11.0.2 Dashboard Tab

- Dashboard migrata come prima scheda interna della Workspace.
- Aggiunto uso reale di WorkspaceHost e WorkspaceTabManager.
- Dashboard pinnata e non chiudibile.
- Nessuna modifica agli altri moduli principali.


## 11.0.1 Workspace Foundation

- Aggiunto ITabContent.
- Aggiunto WorkspaceTab.
- Aggiunto WorkspaceTabManager.
- Aggiunto WorkspaceHost.
- Nessun cambio di comportamento visibile.


## 10.1 RC1 Universal Command Bar

- Aggiunta Universal Command Bar.
- Aggiunto Enterprise Search Service.
- Aggiunto ISearchProvider.
- Aggiunto DigitalTwinSearchProvider.
- Aggiunta integrazione con Action Engine.


## 10.0 RC1.1 Action Engine Syntax Hotfix

- Corretto errore di sintassi in ActionIntentParser.
- Sostituito `or` non valido con operatori C# `||`.
- Stabilizzato parser Action Engine.


## 10.0 RC1 Action Engine Foundation

- Aggiunto Enterprise Action Engine.
- Aggiunto Capability Registry.
- Aggiunti ActionRequest, ActionResult, ActionContext.
- Aggiunto PermissionValidator foundation.
- Aggiunte prime capability Digital Twin.
- Aggiunta finestra AX Action Engine.


## 9.2 AI Intent Catalog Manager

- Aggiunto AiIntentCatalogStorage.
- Aggiunta finestra AI Intent Catalog Manager.
- Catalogo intenti salvato su AppData.
- Aggiunta modifica parole chiave e sinonimi.
- Aggiunto ripristino default.


## 9.1 Enterprise AI Routing Engine

- Aggiunto AiRoutingEngine.
- Aggiunto AiIntentCatalog.
- Aggiunta confidenza risposta.
- Aggiunti sinonimi estesi per Digital Twin.
- L'AI Assistant ora mostra termini riconosciuti e alternative.


## 9.0 Digital Twin Platform Foundation

- Aggiunta Digital Twin Platform.
- Aggiunti modelli DigitalTwinDeviceRecord e DigitalTwinTelemetryRecord.
- Aggiunto DigitalTwinService.
- Aggiunto modulo Digital Twin nella Workspace.
- Aggiunto intento AI Digital Twin.


## 8.5.1 AI Assistant Stability & Data Query Foundation

- Eliminati warning nullable dal motore AI.
- Aggiunto AiDataQueryService.
- Aggiunto AiDataQueryResult.
- Aggiunti intenti HR, Qualità e Manutenzione.
- Aggiunti quick prompt AI.


## 8.5 Enterprise AI Assistant Foundation

- Aggiunto Enterprise AI Assistant.
- Aggiunto AiAssistantEngine.
- Aggiunta classificazione intenti base.
- Aggiunti quick prompts.
- Integrato accesso AI nella Workspace e nella Command Palette.


## 8.4.3 Visual Identity & Responsive Workspace

- Aggiunto icon system foundation.
- Aggiunta adaptive widget grid.
- Aggiunti token dark mode.
- Aggiunto toggle tema foundation nella Workspace.
- Migliorata tipografia Control Room.


## 8.4.2 Premium Workspace UX Polish

- Migliorato design delle card.
- Migliorate spaziature della Control Room.
- Rifinita sidebar Workspace.
- Rifinito editor widget.
- Aggiunti token visuali premium.


## 8.4.1 Widget Layout Hotfix

- Widget più larghi e più alti.
- Scroll interno nei widget con contenuto lungo.
- Corretto overflow del widget Lifecycle Medical.
- Sostituiti eventi obsoleti Checked/Unchecked con IsCheckedChanged.


## 8.4 Widget Engine & Custom Workspace

- Aggiunto Widget Engine.
- Aggiunta Enterprise Control Room.
- Aggiunto editor selezione widget.
- Aggiunto salvataggio layout per utente.
- Aggiunto reset layout.


## 8.3.1 Medical Workspace Hotfix

- Aggiunto `UiTokens.Info`.
- Rimosso riferimento non compatibile a `MedicalDeviceSuiteWindow`.
- Fallback Medical sostituito con navigazione interna alla Workspace.


## 8.3 Medical Suite Workspace Migration

- Aggiunto modulo Medical interno alla Workspace.
- Aggiunti KPI Medical reali.
- Aggiunto lifecycle dispositivo.
- Aggiunti stato operativo ed eventi Digital Twin recenti.
- Mantenuto fallback esterno.


## 8.2 Workspace Module Migration

- Migrata Dashboard dentro Workspace.
- Migrata Analytics dentro Workspace.
- Aggiunti KPI reali e grafici base interni.
- Mantenuti fallback esterni.


## 8.1 Enterprise Workspace Foundation

- Aggiunta Enterprise Workspace.
- Aggiunto NavigationState.
- Aggiunto WorkspaceModuleFactory.
- Aggiunta Command Palette foundation.
- Aggiunta area contenuti centrale dinamica.


## 8.0 Enterprise UI Framework Foundation

- Ripartita da base stabile 7.2.2.
- Aggiunta Enterprise Shell Foundation.
- Aggiunto UIFramework con tokens, components, shell e contracts.
- Aggiunti template XAML non collegati.
- Aggiunte regole ufficiali di refactoring UI.


## 7.2.2 Branded Home Experience

- Aggiunta Branded Home.
- Aggiunti pulsanti rapidi a dashboard, analytics e branding.
- Aggiunti KPI mini nel pannello destro.
- Migliorata esperienza splash/home aziendale.


## 7.2.1 Branded Splash & Login

- Aggiunta schermata Splash/Login brandizzata.
- Aggiunto Branding Center.
- Aggiunta immagine hero personalizzabile.
- Aggiunte preferenze branding in app_settings.


## 7.2 Design System Foundation

- Aggiunti design tokens.
- Aggiunti componenti AxTypography, AxButtons, AxCards, AxBadges, AxLayout.
- Aggiunta finestra Design System Showcase.
- Preparata roadmap di refactoring UI.


## 7.1.2 Apple Style Stable Hotfix

- Sostituita AppleStyleDashboardWindow con versione stabile.
- Corretti errori CS1003.
- Mantenuto layout Apple-like.


## 7.1.1 Apple Style Brush Fix

- Corretto errore CS0029 in AppleStyleDashboardWindow.cs.
- Convertiti colori stringa in Brush tramite Brush.Parse.


## 7.1 Apple Style Enterprise UX

- Aggiunta Apple Style Dashboard.
- Aggiunto tema visuale ispirato a macOS.
- Aggiunte cards arrotondate con ombre leggere.
- Aggiunta sidebar chiara e top bar moderna.


## 7.0 Enterprise UX Foundation

- Aggiunto Enterprise UX Center.
- Aggiunta Top Bar Preview.
- Aggiunti accessi rapidi a ricerca, notifiche, tema e impostazioni.
- Preparata roadmap UX 7.x.


## 6.1.7 Menu Theme Customization

- Aggiunta personalizzazione colori menu.
- Aggiunta personalizzazione hover.
- Aggiunta personalizzazione voce selezionata.
- Aggiornata anteprima tema.


## 6.1.6 Theme & Personalization Center

- Aggiunto modulo Personalizzazione Tema.
- Aggiunte preferenze UI in app_settings.
- Aggiunta anteprima tema.
- Aggiunto audit THEME_PREFERENCES_UPDATED.


## 6.1.5 Collapsible Enterprise Menu

- Menu laterale trasformato in sezioni espandibili.
- Tutte le sezioni sono chiuse all'avvio.
- Aggiunti indicatori `▶` e `▼`.
- Migliorata organizzazione del menu.


## 6.1.4.2 Enterprise Navigation Clean Hotfix

- Ripartita dalla RC 6.1.3 validata.
- Aggiunte icone al menu senza riscrivere AddMenuButton.
- Aggiunta Enterprise Navigation Guide.
- Corretto approccio che causava errori in 6.1.4.1.


## 6.1.3 Analytics Dashboard UX Hotfix

- Pulsante Aggiorna dashboard sempre visibile.
- Grafici spostati più in alto.
- Dashboard più compatta.
- Disabilitato scroll orizzontale nella dashboard.


## 6.1.2 Analytics Charts & Menu UX Fix

- Corretto contrasto del menu laterale.
- Aggiunto tema enterprise antracite/bianco.
- Aggiunto grafico stati dispositivi.
- Aggiunto grafico volumi operativi.
- Aggiunta base chart riutilizzabile.


## 6.1.1 Analytics Dashboard KPI

- Aggiunta nuova Analytics Dashboard.
- Aggiunti KPI operativi.
- Aggiunte notifiche operative base.
- Aggiunti ultimi eventi Digital Twin.
- Aggiunta roadmap Analytics integrata.


## 5.6.2 Enterprise Architecture Crash Fix

- Corretto riferimento tabella `audit_log` in `audit_logs`.
- Aggiunta protezione anti-crash nella finestra Enterprise Architecture.


## 5.6.1 Application Namespace Fix

- Corretto conflitto tra namespace `Accyourate.App.Application` e tipo `Avalonia.Application`.
- `App.cs` ora eredita esplicitamente da `Avalonia.Application`.


## 5.6 Enterprise Architecture Foundation

- Aggiunto appsettings.json.
- Aggiunto logging centralizzato.
- Aggiunto error handling foundation.
- Aggiunto piano migrazioni database.
- Aggiunto Application Health Service.
- Aggiunta API Foundation.
- Aggiunta finestra Enterprise Architecture.


## 5.5.1 Enterprise Responsive UI

- Aggiunto standard ResponsiveUi.
- Aggiunte dimensioni minime alle finestre.
- Migliorato scroll automatico.
- Corretto problema di campi e pulsanti nascosti su finestre piccole.


## 5.5 Enterprise UX & Dashboard

- Aggiunta Enterprise Dashboard.
- Aggiunti KPI principali.
- Aggiunti ultimi eventi Digital Twin.
- Aggiunta Ricerca Globale base.
- Migliorata navigazione verso funzioni enterprise.


## 4.3.1 Menu Scroll Fix

- Corretto menu laterale non scrollabile.
- Le voci in basso sono ora raggiungibili e cliccabili.


## 4.1.5 Architecture & UX Foundation

- Aggiunta architettura modulare ufficiale.
- Aggiunti componenti UI condivisi.
- Aggiunto tema centralizzato.
- Aggiunta finestra Impostazioni.
- Aggiunta finestra Notifiche.
- Aggiunta documentazione UX e Module Contract.
- Preparato backlog RC 4.2 Warehouse & Logistics.


## 4.0.0 Stable - Medical Device Foundation

Versione validata.

### Aggiunto
- Medical Device Suite foundation
- Dispositivi Medici
- Control Unit
- Capi Tessili
- Digital Twin
- Timeline Workflow
- RFID/QR logico
- Export CSV
- Fix `last_insert_rowid()`

### Base consolidata
- Login
- Ruoli e permessi
- Audit
- Workflow Engine
- Backup
- Versionamento database
- Persone
- Asset IT
