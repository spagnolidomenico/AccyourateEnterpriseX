# Accyourate Enterprise X - Sprint 11.0.4 Workspace Stabilization

## Novità

- WorkspaceTabManager centrale per Dashboard e Digital Twin.
- WorkspaceHost unico per i moduli già migrati.
- WorkspaceState foundation.
- Snapshot delle tab aperte.
- Gestione errori modulo con tab dedicata.

## Test rapido

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\test-smoke.ps1
```

## Test funzionale

1. Avvia l'app.
2. Apri Enterprise Workspace.
3. Clicca `Dashboard`.
4. Clicca `Digital Twin`.
5. Verifica che Dashboard e Digital Twin siano tab nello stesso host.
6. Clicca più volte sui menu e verifica che non vengano duplicate.
7. Chiudi Digital Twin.
8. Verifica Universal Command Bar, AX Action Engine, AI Assistant e Control Room.

---

# Accyourate Enterprise X - Sprint 11.0.3 Digital Twin Tab

## Novità

- Digital Twin migrato come scheda interna della Workspace.
- Uso reale di `WorkspaceTabManager` e `WorkspaceHost` anche per Digital Twin.
- Scheda Digital Twin chiudibile.
- Nessuna modifica funzionale a AI Assistant, Action Engine e Universal Command Bar.

## Test rapido

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\test-smoke.ps1
```

## Test funzionale

1. Avvia l'app.
2. Apri Enterprise Workspace.
3. Clicca `Digital Twin`.
4. Verifica che Digital Twin sia mostrato dentro una scheda.
5. Clicca nuovamente `Digital Twin` e verifica che non venga duplicato.
6. Chiudi la scheda Digital Twin.
7. Verifica Dashboard, Universal Command Bar, AX Action Engine e AI Assistant.

---

# Accyourate Enterprise X - Sprint 11.0.2 Dashboard Tab

## Novità

- Dashboard migrata come prima scheda interna della Workspace.
- Uso reale di `WorkspaceTabManager` e `WorkspaceHost`.
- Scheda Dashboard pinnata e non chiudibile.
- Nessuna modifica funzionale agli altri moduli.

## Test rapido

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\test-smoke.ps1
```

## Test funzionale

1. Avvia l'app.
2. Apri Enterprise Workspace.
3. Clicca `Dashboard`.
4. Verifica che la Dashboard sia mostrata dentro una scheda.
5. Clicca nuovamente `Dashboard` e verifica che non venga duplicata.
6. Verifica Digital Twin, Universal Command Bar, AX Action Engine e AI Assistant.

---

# Accyourate Enterprise X - Sprint 11.0.1 Workspace Foundation

## Novità

- ITabContent
- WorkspaceTab
- WorkspaceTabManager
- WorkspaceHost
- Documentazione sprint 11.0.1
- ADR Workspace Tabs Foundation

## Nota importante

Questa versione **non cambia ancora il comportamento visibile della Workspace**.
Introduce solo l'infrastruttura tecnica per le future schede.

## Test rapido

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\test-smoke.ps1
```

oppure:

```powershell
dotnet clean AccyourateEnterpriseX.sln
dotnet restore AccyourateEnterpriseX.sln
dotnet build AccyourateEnterpriseX.sln
```

## Test funzionale

1. Avvia l'app.
2. Apri Enterprise Workspace.
3. Verifica che funzioni come nella 10.1 RC1.
4. Apri Digital Twin.
5. Apri Universal Command Bar.
6. Apri AX Action Engine.
7. Apri AI Assistant.

---

# Accyourate Enterprise X - Versione 10.1 RC1 Universal Command Bar

## Novità

- Universal Command Bar
- Enterprise Search Service
- ISearchProvider
- DigitalTwinSearchProvider
- SearchResult standardizzato
- Integrazione con Action Engine
- Accesso da Workspace, Command Palette e menu principale

## Test rapido

```powershell
dotnet clean AccyourateEnterpriseX.sln
dotnet restore AccyourateEnterpriseX.sln
dotnet build AccyourateEnterpriseX.sln
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

## Test funzionale

1. Apri `Universal Command Bar`.
2. Prova a cercare:
   - TOP
   - TOP001
   - offline
   - batteria
   - ECG
   - telemetria
3. Seleziona un risultato.
4. Verifica messaggio di esecuzione Action Engine.
5. Verifica regressione su AX Action Engine e Digital Twin.

---

# Accyourate Enterprise X - Versione 10.0 RC1.1 Action Engine Syntax Hotfix

## Correzione

Questa hotfix corregge l'errore di sintassi in:

```text
src/Accyourate.App/ActionEngine/ActionIntentParser.cs
```

## Corretto

- sostituita sintassi non valida `or` con operatori C# validi `||`;
- stabilizzato il parsing per batteria, offline, ECG, telemetria e apertura dispositivo;
- aggiornata versione a `10.0 RC1.1`.

## Test rapido

```powershell
dotnet clean AccyourateEnterpriseX.sln
dotnet restore AccyourateEnterpriseX.sln
dotnet build AccyourateEnterpriseX.sln
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

## Test Action Engine

Prova:

```text
Apri il Digital Twin del dispositivo TOP001
Mostrami dispositivi con batteria sotto il 20%
Mostrami dispositivi offline
Mostra telemetria TOP001
Mostra ECG TOP001
```

---

# Accyourate Enterprise X - Versione 10.0 RC1 Action Engine Foundation

## Novità

- Enterprise Action Engine
- Capability Registry
- Action Request / Result / Context
- Permission Validator foundation
- Action Intent Parser
- Prime capability Digital Twin
- Finestra Action Engine
- Integrazione con Workspace, AI Assistant e Command Palette

## Test rapido

```powershell
dotnet clean AccyourateEnterpriseX.sln
dotnet restore AccyourateEnterpriseX.sln
dotnet build AccyourateEnterpriseX.sln
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

## Test funzionale

1. Apri `AX Action Engine`.
2. Verifica capability registrate.
3. Prova:
   - Apri il Digital Twin del dispositivo TOP001
   - Mostrami dispositivi con batteria sotto il 20%
   - Mostrami dispositivi offline
   - Mostra telemetria TOP001
   - Mostra ECG TOP001
4. Verifica Workspace, Digital Twin e AI Assistant.

---

# Accyourate Enterprise X - Versione 9.2 AI Intent Catalog Manager

## Novità

- Catalogo intenti AI modificabile
- Salvataggio su AppData
- Ripristino default
- Gestione sinonimi Digital Twin
- AI Routing Engine configurabile

## Test rapido

```powershell
dotnet clean AccyourateEnterpriseX.sln
dotnet restore AccyourateEnterpriseX.sln
dotnet build AccyourateEnterpriseX.sln
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

## Test funzionale

1. Apri AI Intent Catalog.
2. Seleziona Digital Twin.
3. Aggiungi un sinonimo.
4. Salva.
5. Apri AI Assistant.
6. Prova il nuovo sinonimo.
7. Ripristina catalogo default.

---

# Accyourate Enterprise X - Versione 9.1 Enterprise AI Routing Engine

## Novità

- AI Routing Engine
- Catalogo intenti centralizzato
- Sinonimi Digital Twin estesi
- Confidenza risposta
- Termini riconosciuti
- Alternative per richieste ambigue

## Test rapido

```powershell
dotnet clean AccyourateEnterpriseX.sln
dotnet restore AccyourateEnterpriseX.sln
dotnet build AccyourateEnterpriseX.sln
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

## Test funzionale

1. Apri AI Assistant.
2. Prova:
   - Mostrami i digital twin
   - Telemetria dispositivi
   - ECG
   - Monitoraggio cardiaco
   - Battito cardiaco
   - Capo tessile intelligente
   - Bluetooth
   - RFID
   - Batteria
3. Verifica che venga suggerito Digital Twin.

---

# Accyourate Enterprise X - Versione 9.0 Digital Twin Platform Foundation

## Novità

- Digital Twin Platform
- Capi tessili medicali intelligenti
- ECG e battito cardiaco
- Telemetria
- Batteria e qualità segnale
- Lifecycle Digital Twin
- AI intent Digital Twin

## Test rapido

```powershell
dotnet clean AccyourateEnterpriseX.sln
dotnet restore AccyourateEnterpriseX.sln
dotnet build AccyourateEnterpriseX.sln
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

## Test funzionale

1. Apri `Enterprise Workspace`.
2. Clicca `Digital Twin`.
3. Verifica KPI, tabella dispositivi, monitoraggio clinico e telemetry feed.
4. Apri AI Assistant e chiedi: `Mostrami i digital twin`.
5. Prova Command Palette cercando `Digital Twin`.

---

# Accyourate Enterprise X - Versione 8.5.1 AI Assistant Stability & Data Query Foundation

## Novità

- AI Assistant più stabile
- Eliminazione warning nullable nel motore AI
- AiDataQueryService
- Intenti HR, Qualità e Manutenzione
- Quick prompt aggiuntivi
- Base per query dati sicure

## Test rapido

```powershell
dotnet clean AccyourateEnterpriseX.sln
dotnet restore AccyourateEnterpriseX.sln
dotnet build AccyourateEnterpriseX.sln
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

## Test funzionale

1. Apri `Enterprise Workspace`.
2. Clicca `AI`.
3. Prova:
   - Quanti dispositivi medici ci sono?
   - Mostrami i documenti
   - Asset IT disponibili
   - Quanti test qualità ci sono?
   - Manutenzioni aperte
4. Verifica che non compaiano errori.

---

# Accyourate Enterprise X - Versione 8.5 Enterprise AI Assistant Foundation

## Novità

- Enterprise AI Assistant
- Intent detection base
- Suggerimenti operativi
- Quick prompts
- Accesso dalla Workspace
- Accesso dalla Command Palette

## Test rapido

```powershell
dotnet clean AccyourateEnterpriseX.sln
dotnet restore AccyourateEnterpriseX.sln
dotnet build AccyourateEnterpriseX.sln
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

## Test funzionale

1. Apri `Enterprise Workspace`.
2. Clicca `AI` nella top bar.
3. Prova i quick prompt.
4. Scrivi `Quanti dispositivi medici ci sono?`.
5. Scrivi `Mostrami i documenti`.
6. Apri Command Palette e cerca `AI`.

---

# Accyourate Enterprise X - Versione 8.4.3 Visual Identity & Responsive Workspace

## Novità

- Icon system foundation
- Icone più coerenti
- Tipografia migliorata
- Adaptive widget grid
- Tema chiaro/scuro foundation
- Workspace più premium e scalabile

## Test rapido

```powershell
dotnet clean AccyourateEnterpriseX.sln
dotnet restore AccyourateEnterpriseX.sln
dotnet build AccyourateEnterpriseX.sln
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

## Test funzionale

1. Apri `Enterprise Workspace`.
2. Verifica le icone nella sidebar.
3. Apri `Control Room`.
4. Verifica griglia e spaziatura widget.
5. Premi `Tema` nella top bar.
6. Prova navigazione tra moduli.

---

# Accyourate Enterprise X - Versione 8.4.2 Premium Workspace UX Polish

## Novità

- Workspace più premium
- Control Room più ordinata
- Card più eleganti
- Spaziature migliorate
- Editor widget più leggibile
- Design System più coerente

## Test rapido

```powershell
dotnet clean AccyourateEnterpriseX.sln
dotnet restore AccyourateEnterpriseX.sln
dotnet build AccyourateEnterpriseX.sln
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

## Test funzionale

1. Apri `Enterprise Workspace`.
2. Apri `Control Room`.
3. Controlla spaziature, card e leggibilità.
4. Apri `Personalizza widget`.
5. Verifica che il layout resti salvato.
6. Prova Dashboard, Analytics e Medical.

---

# Accyourate Enterprise X - Versione 8.4.1 Widget Layout Hotfix

## Correzioni

- Aumentata dimensione dei widget nella Control Room.
- Aggiunto scroll interno ai widget con contenuto più lungo.
- Migliorata leggibilità del widget Lifecycle Medical.
- Rimossi warning Avalonia `Checked/Unchecked` obsoleti usando `IsCheckedChanged`.

## Test rapido

```powershell
dotnet clean AccyourateEnterpriseX.sln
dotnet restore AccyourateEnterpriseX.sln
dotnet build AccyourateEnterpriseX.sln
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

## Test

1. Apri `Enterprise Workspace`.
2. Vai in `Control Room`.
3. Controlla `Lifecycle Medical`.
4. Verifica che il contenuto non esca dal riquadro.
5. Apri `Personalizza widget` e salva il layout.

---

# Accyourate Enterprise X - Versione 8.4 Widget Engine & Custom Workspace

## Novità

- Widget Engine
- Enterprise Control Room
- Personalizzazione widget per utente
- Salvataggio layout
- Reset layout
- Prime dashboard specifiche a widget

## Test rapido

```powershell
dotnet clean AccyourateEnterpriseX.sln
dotnet restore AccyourateEnterpriseX.sln
dotnet build AccyourateEnterpriseX.sln
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

## Test funzionale

1. Apri `Enterprise Workspace`.
2. Clicca `Control Room`.
3. Clicca `Personalizza widget`.
4. Deseleziona/seleziona alcuni widget.
5. Salva.
6. Riapri Control Room e verifica il layout.
7. Prova Reset layout.

---

# Accyourate Enterprise X - Versione 8.3.1 Medical Workspace Hotfix

## Correzioni

Questa hotfix corregge solo gli errori di compilazione della 8.3.

### Corretto

- Aggiunto `UiTokens.Info`.
- Rimosso riferimento diretto a `MedicalDeviceSuiteWindow`.
- Il fallback Medical ora rimanda al modulo Medical interno alla Workspace.
- Aggiornata versione Workspace a `8.3.1`.

## Test rapido

```powershell
dotnet clean AccyourateEnterpriseX.sln
dotnet restore AccyourateEnterpriseX.sln
dotnet build AccyourateEnterpriseX.sln
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

---

# Accyourate Enterprise X - Versione 8.3 Medical Suite Workspace Migration

## Novità

- Medical Device Suite dentro la Workspace
- KPI medical reali
- Lifecycle dispositivo
- Stato operativo Medical Suite
- Eventi Digital Twin recenti
- Fallback esterno mantenuto

## Test rapido

```powershell
dotnet clean AccyourateEnterpriseX.sln
dotnet restore AccyourateEnterpriseX.sln
dotnet build AccyourateEnterpriseX.sln
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

## Test funzionale

1. Apri `Enterprise Workspace`.
2. Clicca `Medical`.
3. Verifica KPI, lifecycle, stato operativo ed eventi.
4. Prova `⌘K Comandi` e cerca Medical.
5. Apri il fallback Medical Device Suite esterno.

---

# Accyourate Enterprise X - Versione 8.2 Workspace Module Migration

## Novità

- Dashboard migrata nella Workspace
- Analytics migrata nella Workspace
- KPI reali interni
- Grafici base interni
- Stato sistemi
- Accessi rapidi

## Test rapido

```powershell
dotnet clean AccyourateEnterpriseX.sln
dotnet restore AccyourateEnterpriseX.sln
dotnet build AccyourateEnterpriseX.sln
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

## Test funzionale

1. Apri `Enterprise Workspace`.
2. Clicca Dashboard.
3. Clicca Analytics.
4. Verifica KPI, grafici e stato sistemi.
5. Prova Command Palette.
6. Verifica fallback finestre esterne.

---

# Accyourate Enterprise X - Versione 8.1 Enterprise Workspace Foundation

## Novità

- Enterprise Workspace
- Navigation Service foundation
- Command Palette
- Area centrale dinamica
- Status bar
- Sidebar e top bar persistenti

## Test rapido

```powershell
dotnet clean AccyourateEnterpriseX.sln
dotnet restore AccyourateEnterpriseX.sln
dotnet build AccyourateEnterpriseX.sln
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

## Test funzionale

1. Apri `Enterprise Workspace`.
2. Cambia modulo nella sidebar.
3. Prova `⌘K Comandi`.
4. Apri un modulo dalla Command Palette.
5. Prova i collegamenti esterni.
6. Verifica regressione sui moduli già validati.

---

# Accyourate Enterprise X - Versione 8.0 Enterprise UI Framework Foundation

## Novità

- Enterprise UI Framework Foundation
- Shell foundation
- Registry moduli UI
- Component factory
- Design tokens UI
- Template XAML non ancora collegati
- Regole ufficiali di refactoring UI

## Test rapido

```powershell
dotnet clean AccyourateEnterpriseX.sln
dotnet restore AccyourateEnterpriseX.sln
dotnet build AccyourateEnterpriseX.sln
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

## Test funzionale

1. Apri `Enterprise Shell Foundation`.
2. Verifica sidebar, top bar, cards e roadmap.
3. Verifica che i moduli già validati continuino a funzionare.
4. Non testare la 7.3 precedente: questa 8.0 riparte dalla base stabile 7.2.2.

---

# Accyourate Enterprise X - Versione 7.2.2 Branded Home Experience

## Novità

- Branded Home
- Splash/Login Branding migliorato
- Accesso rapido a dashboard e branding
- KPI mini
- Hero aziendale configurabile
- Moduli in evidenza

## Test rapido

```powershell
dotnet clean AccyourateEnterpriseX.sln
dotnet restore AccyourateEnterpriseX.sln
dotnet build AccyourateEnterpriseX.sln
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

## Test funzionale

1. Apri `Branded Home`.
2. Verifica immagine hero e testi.
3. Prova i pulsanti rapidi.
4. Apri `Branding Center`.
5. Cambia nome azienda o immagine.
6. Riapri `Branded Home`.

---

# Accyourate Enterprise X - Versione 7.2.1 Branded Splash & Login

## Novità

- Splash/Login personalizzabile
- Branding Center
- Immagine hero aziendale
- Logo e nome azienda
- Messaggio introduttivo
- Moduli in evidenza

## Test rapido

```powershell
dotnet clean AccyourateEnterpriseX.sln
dotnet restore AccyourateEnterpriseX.sln
dotnet build AccyourateEnterpriseX.sln
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

## Test funzionale

1. Apri `Splash/Login Branding`.
2. Verifica la schermata con immagine hero.
3. Apri `Branding Center`.
4. Modifica nome azienda o immagine hero.
5. Salva.
6. Riapri `Splash/Login Branding`.

---

# Accyourate Enterprise X - Versione 7.2 Design System Foundation

## Novità

- Design System Foundation
- Design tokens
- Componenti comuni
- Card, pulsanti, badge, layout e typography
- Design System Showcase

## Test rapido

```powershell
dotnet clean AccyourateEnterpriseX.sln
dotnet restore AccyourateEnterpriseX.sln
dotnet build AccyourateEnterpriseX.sln
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

---

# Accyourate Enterprise X - Versione 7.1.2 Apple Style Stable Hotfix

## Correzione

Sostituita `AppleStyleDashboardWindow.cs` con una versione stabile e più semplice.

Corregge:
- errori di sintassi `CS1003`;
- errori di Brush;
- dashboard Apple Style non compilabile.

## Mantiene

- Apple Style Dashboard;
- sidebar chiara;
- top bar moderna;
- cards arrotondate;
- KPI eleganti;
- layout ispirato al mockup approvato.

## Test rapido

```powershell
dotnet clean AccyourateEnterpriseX.sln
dotnet restore AccyourateEnterpriseX.sln
dotnet build AccyourateEnterpriseX.sln
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

---

# Accyourate Enterprise X - Versione 7.1.1 Apple Style Brush Fix

## Correzione

Risolto errore di compilazione:

```text
CS0029: Non è possibile convertire implicitamente il tipo 'string' in 'Avalonia.Media.IBrush'
```

Causa:
- in `AppleStyleDashboardWindow.cs` alcuni colori erano passati come stringhe;
- Avalonia richiede `IBrush`.

Correzione:
- conversione tramite `Brush.Parse(...)`.

## Test rapido

```powershell
dotnet clean AccyourateEnterpriseX.sln
dotnet restore AccyourateEnterpriseX.sln
dotnet build AccyourateEnterpriseX.sln
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

---

# Accyourate Enterprise X - Versione 7.1 Apple Style Enterprise UX

## Novità

- Apple Style Dashboard
- Interfaccia chiara stile macOS
- Sidebar moderna
- Top bar moderna
- Cards arrotondate
- KPI più eleganti
- Base per icone e grafica Apple-like

## Test rapido

```powershell
dotnet clean AccyourateEnterpriseX.sln
dotnet restore AccyourateEnterpriseX.sln
dotnet build AccyourateEnterpriseX.sln
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

## Test funzionale

1. Apri `Apple Style Dashboard`.
2. Verifica sidebar, top bar, KPI, grafici e cards.
3. Ridimensiona la finestra.
4. Confronta il risultato con il mockup approvato.
5. Verifica regressione sugli altri moduli.

---

# Accyourate Enterprise X - Versione 7.0 Enterprise UX Foundation

## Novità

- Enterprise UX Center
- Top Bar Preview
- Accesso rapido a ricerca, notifiche, tema e impostazioni
- Base per preferiti, recenti, ricerca menu e temi live

## Test rapido

```powershell
dotnet clean AccyourateEnterpriseX.sln
dotnet restore AccyourateEnterpriseX.sln
dotnet build AccyourateEnterpriseX.sln
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

## Test funzionale

1. Apri Enterprise UX Center.
2. Apri Top Bar Preview.
3. Prova Cerca globale.
4. Prova Notifiche.
5. Prova Tema.
6. Prova Impostazioni.
7. Verifica regressione sui moduli principali.

---

# Accyourate Enterprise X - RC 6.1.7 Menu Theme Customization

## Novità

- Personalizzazione voce menu normale
- Personalizzazione hover menu
- Personalizzazione voce selezionata
- Anteprima aggiornata
- Salvataggio preferenze in database

## Test rapido

```powershell
dotnet clean AccyourateEnterpriseX.sln
dotnet restore AccyourateEnterpriseX.sln
dotnet build AccyourateEnterpriseX.sln
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

---

# Accyourate Enterprise X - RC 6.1.6 Theme & Personalization Center

## Novità

- Modulo Personalizzazione Tema
- Nome azienda configurabile
- Tema chiaro/scuro/sistema
- Colore primario configurabile
- Colore menu e area lavoro
- Percorso logo
- Anteprima tema
- Salvataggio in database

## Test rapido

```powershell
dotnet clean AccyourateEnterpriseX.sln
dotnet restore AccyourateEnterpriseX.sln
dotnet build AccyourateEnterpriseX.sln
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

## Test funzionale

1. Apri `Amministrazione`.
2. Apri `Personalizzazione Tema`.
3. Cambia colore primario.
4. Verifica anteprima.
5. Salva.
6. Riapri la finestra e verifica che le preferenze siano mantenute.

---

# Accyourate Enterprise X - RC 6.1.5 Collapsible Enterprise Menu

## Novità

- Menu a sezioni espandibili.
- Tutte le sezioni chiuse all'avvio.
- Navigazione più ordinata.
- Indicatori `▶` e `▼`.

## Test rapido

```powershell
dotnet clean AccyourateEnterpriseX.sln
dotnet restore AccyourateEnterpriseX.sln
dotnet build AccyourateEnterpriseX.sln
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

---

# Accyourate Enterprise X - RC 6.1.4.2 Enterprise Navigation Clean Hotfix

## Correzione

Ripartita dalla RC 6.1.3 validata e applicate solo modifiche sicure al menu.

## Aggiunge

- Icone nel menu
- Sezioni più intuitive
- Enterprise Navigation Guide
- Migliore contrasto

## Test rapido

```powershell
dotnet clean AccyourateEnterpriseX.sln
dotnet restore AccyourateEnterpriseX.sln
dotnet build AccyourateEnterpriseX.sln
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

---

# Accyourate Enterprise X - RC 6.1.3 Analytics Dashboard UX Hotfix

## Correzioni

- Pulsante `Aggiorna dashboard` reso sempre visibile.
- Grafici spostati più in alto.
- Dashboard più compatta.
- Scroll orizzontale disabilitato nella dashboard.
- KPI ridotti per migliorare la leggibilità.

## Test rapido

```powershell
dotnet clean AccyourateEnterpriseX.sln
dotnet restore AccyourateEnterpriseX.sln
dotnet build AccyourateEnterpriseX.sln
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

---

# Accyourate Enterprise X - RC 6.1.2 Analytics Charts & Menu UX Fix

## Novità

- Correzione leggibilità menu laterale
- Tema enterprise antracite/bianco
- Grafico stati dispositivi
- Grafico volumi operativi
- Base chart riutilizzabile

## Test rapido

```powershell
dotnet clean AccyourateEnterpriseX.sln
dotnet restore AccyourateEnterpriseX.sln
dotnet build AccyourateEnterpriseX.sln
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

## Test funzionale

1. Accedi come admin.
2. Verifica che il menu laterale sia leggibile.
3. Apri Analytics Dashboard.
4. Verifica KPI, grafici, notifiche ed eventi.
5. Ridimensiona la finestra.
6. Verifica regressione sui moduli principali.

---

# Accyourate Enterprise X - RC 6.1.1 Analytics Dashboard KPI

## Novità

- Analytics Dashboard KPI
- Notifiche operative base
- Ultimi eventi Digital Twin
- Roadmap Analytics
- Layout pronto per grafici futuri

## Test rapido

```powershell
dotnet clean AccyourateEnterpriseX.sln
dotnet restore AccyourateEnterpriseX.sln
dotnet build AccyourateEnterpriseX.sln
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

## Test funzionale

1. Accedi come admin.
2. Apri `Analytics Dashboard`.
3. Verifica che i KPI siano visibili.
4. Verifica gli ultimi eventi Digital Twin.
5. Verifica le notifiche operative.
6. Premi `Aggiorna`.
7. Verifica regressione sui moduli principali.

---

# Accyourate Enterprise X - Versione 5.6.2 Enterprise Architecture Crash Fix

## Correzione

Risolto crash aprendo `Enterprise Architecture`.

Causa:
- la finestra cercava la tabella `audit_log`;
- la tabella corretta nel database è `audit_logs`.

Correzioni:
- `audit_log` → `audit_logs`;
- aggiunta protezione anti-crash nella finestra Enterprise Architecture.

## Test rapido

```powershell
dotnet clean AccyourateEnterpriseX.sln
dotnet restore AccyourateEnterpriseX.sln
dotnet build AccyourateEnterpriseX.sln
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

## Test

1. Accedi come admin.
2. Apri `Enterprise Architecture`.
3. Verifica che la finestra non chiuda il gestionale.
4. Verifica Health Report, migrazioni e API Foundation.

---

# Accyourate Enterprise X - Versione 5.6.1 Application Namespace Fix

## Correzione

Risolto errore di compilazione:

```text
CS0118: 'Application' è spazio dei nomi ma è usato come tipo
```

Causa:
- la 5.6 ha introdotto il namespace `Accyourate.App.Application`;
- `App.cs` usava `Application` senza qualificarlo;
- il compilatore confondeva il namespace interno con `Avalonia.Application`.

Soluzione:
- `App.cs` ora usa esplicitamente `Avalonia.Application`.

## Test rapido

```powershell
dotnet clean AccyourateEnterpriseX.sln
dotnet restore AccyourateEnterpriseX.sln
dotnet build AccyourateEnterpriseX.sln
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

---

# Accyourate Enterprise X - Versione 5.6 Enterprise Architecture Foundation

## Novità

- Application layer foundation
- Infrastructure layer foundation
- appsettings.json
- Logging centralizzato
- Error handling foundation
- Database Migration Plan
- API Foundation
- Application Health Service
- Finestra Enterprise Architecture

## Test rapido

```powershell
dotnet clean AccyourateEnterpriseX.sln
dotnet restore AccyourateEnterpriseX.sln
dotnet build AccyourateEnterpriseX.sln
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

## Test funzionale

1. Accedi come admin.
2. Apri `Enterprise Architecture`.
3. Verifica Health Report.
4. Verifica Piano Migrazioni.
5. Verifica API Foundation.
6. Verifica che tutti i moduli precedenti funzionino.

---

# Accyourate Enterprise X - Versione 5.5.1 Enterprise Responsive UI

## Correzione principale

Risolto il problema delle finestre che, se non sufficientemente grandi, nascondevano campi o pulsanti.

## Migliorie

- Dimensioni minime più sicure.
- Scroll verticale/orizzontale automatico.
- Standard responsive condiviso.
- Layout più robusto per finestre complesse.
- Documentazione UX responsive.

## Test rapido

```powershell
dotnet clean AccyourateEnterpriseX.sln
dotnet restore AccyourateEnterpriseX.sln
dotnet build AccyourateEnterpriseX.sln
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

## Test funzionale

1. Apri tutti i moduli principali.
2. Riduci la finestra.
3. Verifica che appaiano le barre di scorrimento.
4. Verifica che i pulsanti restino raggiungibili.
5. Verifica che il menu laterale continui a scorrere.
6. Verifica che non ci siano regressioni.

---

# Accyourate Enterprise X - Versione 5.5 Enterprise UX & Dashboard

## Novità

- Enterprise Dashboard
- KPI operativi
- Ultimi eventi Digital Twin
- Ricerca globale
- UX più orientata a prodotto enterprise

## Test rapido

```powershell
dotnet clean AccyourateEnterpriseX.sln
dotnet restore AccyourateEnterpriseX.sln
dotnet build AccyourateEnterpriseX.sln
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

## Test funzionale

1. Accedi come admin.
2. Apri `Enterprise Dashboard`.
3. Verifica KPI e ultimi eventi.
4. Apri `Ricerca Globale`.
5. Cerca un dispositivo, una persona, un asset o un documento.
6. Verifica che tutti i moduli precedenti continuino a funzionare.

---

# Accyourate Enterprise X - Versione 5.0 Document Management Foundation

## Novità

- Archivio documentale centralizzato
- Documenti collegabili a dispositivi, persone e asset IT
- Categorie documentali
- Versionamento base
- Generazione TXT
- Collegamento al Digital Twin
- Audit documentale

## Test rapido

```powershell
dotnet clean AccyourateEnterpriseX.sln
dotnet restore AccyourateEnterpriseX.sln
dotnet build AccyourateEnterpriseX.sln
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

## Test funzionale

1. Accedi come admin.
2. Apri `Document Management`.
3. Crea un documento `DOC001`.
4. Collegalo a un dispositivo medico.
5. Premi `TXT`.
6. Apri il Digital Twin del dispositivo.
7. Verifica evento `DOCUMENT_ATTACHED`.
8. Archivia il documento.
9. Verifica audit.

---

# Accyourate Enterprise X - RC 4.3.1 Menu Scroll Fix

## Correzione

Il menu laterale ora è inserito in uno ScrollViewer verticale.

Questo risolve il problema delle voci in basso non cliccabili quando il menu supera l'altezza della finestra.

## Test rapido

```powershell
dotnet clean AccyourateEnterpriseX.sln
dotnet restore AccyourateEnterpriseX.sln
dotnet build AccyourateEnterpriseX.sln
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

## Test funzionale

1. Accedi come admin.
2. Riduci l'altezza della finestra.
3. Verifica che il menu laterale mostri la barra di scorrimento.
4. Scorri fino in fondo.
5. Verifica che le ultime voci siano cliccabili.

---

# Accyourate Enterprise X - RC 4.3 Laundry & Maintenance

## Novità

- Cicli di lavaggio
- Contatore lavaggi
- Soglia lavaggi base
- Manutenzioni e riparazioni
- Fuori servizio / rientro in servizio
- Timeline Digital Twin automatica
- Audit Laundry & Maintenance

## Test rapido

```powershell
dotnet clean AccyourateEnterpriseX.sln
dotnet restore AccyourateEnterpriseX.sln
dotnet build AccyourateEnterpriseX.sln
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

## Test funzionale

1. Accedi come admin.
2. Usa o crea un dispositivo medico con capo tessile.
3. Apri `Laundry & Maintenance`.
4. Registra lavaggio `LAV001`.
5. Registra manutenzione `MAN001`.
6. Apri il Digital Twin del dispositivo.
7. Verifica gli eventi lavaggio/manutenzione nella timeline.

---

# Accyourate Enterprise X - RC 4.2 Warehouse & Logistics

## Novità

- Ubicazioni di magazzino
- Movimentazioni dispositivi
- Spedizioni e rientri
- Timeline Digital Twin automatica
- Audit Warehouse & Logistics

## Test rapido

```powershell
dotnet clean AccyourateEnterpriseX.sln
dotnet restore AccyourateEnterpriseX.sln
dotnet build AccyourateEnterpriseX.sln
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

## Test funzionale

1. Accedi come admin.
2. Crea o usa un dispositivo medico.
3. Apri `Warehouse & Logistics`.
4. Crea ubicazione `MAG-A-01`.
5. Registra movimentazione `Entrata`.
6. Crea spedizione `SPED001`.
7. Premi `Rientro`.
8. Apri il Digital Twin del dispositivo.
9. Verifica gli eventi logistici nella timeline.

---

# Accyourate Enterprise X - 4.1.5 Architecture & UX Foundation

## Obiettivo

Questa versione consolida l'architettura e l'esperienza utente prima della RC 4.2 Warehouse & Logistics.

## Aggiunge

- architettura modulare ufficiale;
- cartelle Core / Medical / IT / HR / Documents / Reports;
- tema centralizzato;
- componenti UI riutilizzabili;
- finestra Impostazioni;
- finestra Notifiche;
- standard UX;
- contratto modulo;
- backlog RC 4.2.

## Test rapido

```powershell
dotnet clean AccyourateEnterpriseX.sln
dotnet restore AccyourateEnterpriseX.sln
dotnet build AccyourateEnterpriseX.sln
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

## Test funzionale

1. Login admin.
2. Verifica Dashboard.
3. Apri Persone.
4. Apri Asset IT.
5. Apri Dispositivi Medici.
6. Apri Production & Quality.
7. Apri Impostazioni.
8. Apri Notifiche.
9. Apri Infrastruttura.
10. Verifica che non ci siano regressioni.

---

# Accyourate Enterprise X - RC 4.1 Production & Quality Suite

## Novità

- Ordini di produzione
- Avanzamento produzione
- Controllo qualità
- Checklist base
- Timeline Digital Twin automatica
- Audit produzione/qualità

## Test rapido

```powershell
dotnet clean AccyourateEnterpriseX.sln
dotnet restore AccyourateEnterpriseX.sln
dotnet build AccyourateEnterpriseX.sln
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

## Test funzionale

1. Accedi come admin.
2. Crea o usa un dispositivo medico.
3. Apri `Production & Quality`.
4. Crea ordine `PROD001`.
5. Premi `Avvia`.
6. Premi `Completa`.
7. Crea test qualità `QT001` con esito `Conforme`.
8. Apri il Digital Twin del dispositivo.
9. Verifica la timeline.

---

# Accyourate Enterprise X

## Versione

**4.0.0 Stable Git Ready**

Questa è la baseline stabile validata prima dello sviluppo della RC 4.1.

## Moduli validati

- Login
- Utenti, ruoli e permessi
- Audit
- Diagnostica
- Backup
- Workflow Engine
- Persone
- Asset IT
- Medical Device Foundation
- Control Unit
- Capi Tessili
- Digital Twin

## Test rapido

```powershell
dotnet clean AccyourateEnterpriseX.sln
dotnet restore AccyourateEnterpriseX.sln
dotnet build AccyourateEnterpriseX.sln
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

## Git setup

Leggere:

```text
docs\git\GIT_SETUP_GUIDE.md
```

Oppure eseguire:

```text
scripts\git\01_init_repo.bat
```

## Prossimo sviluppo

RC 4.1 - Production & Quality Suite.

---

# Accyourate Enterprise X 2026 - Compilable Base

Questa versione sostituisce lo scheletro precedente con un progetto più piccolo ma realmente compilabile.

## Requisiti

- .NET SDK 9

## Comandi corretti

Apri PowerShell nella cartella principale ed esegui:

```powershell
dotnet restore AccyourateEnterpriseX.sln
dotnet build AccyourateEnterpriseX.sln
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

Oppure usa gli script:

```text
scripts\restore.bat
scripts\build.bat
scripts\run.bat
```

## Credenziali demo

```text
admin
admin123
```

## Cosa contiene

- Progetto Avalonia reale
- Login demo funzionante
- Finestra principale
- Layout Accyourate
- Menu laterale
- Centro Operativo base

## Prossimo step

Aggiunta SQLite, utenti reali, ruoli, permessi e menu dinamico.


# Fix 1

Corretto errore di compilazione:

```text
CS0246: UniformGrid non trovato
```

`UniformGrid` è stato sostituito con un `Grid` standard Avalonia.

## Comandi

```powershell
dotnet restore AccyourateEnterpriseX.sln
dotnet build AccyourateEnterpriseX.sln
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```


# Alpha 0.2 Database Login

Questa versione aggiunge SQLite, database in ProgramData, tabella utenti, audit log e login collegato al database.

## Database

```text
C:\ProgramData\Accyourate Enterprise X\data\accyourate_x.db
```

## Credenziali iniziali

```text
admin
admin123
```

## Comandi

```powershell
dotnet restore AccyourateEnterpriseX.sln
dotnet build AccyourateEnterpriseX.sln
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```


# Alpha 0.3 Gestione Utenti

Aggiunto:

- schermata Gestione Utenti;
- creazione nuovo utente;
- cambio ruolo base Admin/Operatore;
- attiva/disattiva utente;
- blocco disattivazione admin principale;
- schermata Diagnostica Database;
- visualizzazione ultimi eventi audit.

## Come provarla

1. Avvia il programma.
2. Accedi con `admin / admin123`.
3. Nel menu laterale clicca **Gestione Utenti**.
4. Crea un nuovo utente.
5. Esci e prova ad accedere con il nuovo utente.
6. Apri **Diagnostica** per vedere percorso DB, utenti e audit.


# Developer Edition 1.0

Questa versione corregge gli errori Avalonia relativi a:

```text
Grid.ColumnSpacing
Grid.RowSpacing
```

e consolida la base con:

- login collegato al database SQLite;
- database in ProgramData;
- gestione utenti;
- diagnostica database;
- audit log base;
- script `clean-build-run.bat`.

## Test consigliato

```powershell
dotnet clean AccyourateEnterpriseX.sln
dotnet restore AccyourateEnterpriseX.sln
dotnet build AccyourateEnterpriseX.sln
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

Oppure:

```text
scripts\clean-build-run.bat
```

## Credenziali iniziali

```text
admin
admin123
```


# Developer Edition 1.1 - Ruoli, Permessi e Cambio Password

Aggiunge:

- tabella `roles`;
- tabella `permissions`;
- tabella `role_permissions`;
- seed ruoli Admin, Operatore, Lettura;
- seed permessi base;
- menu filtrato in base ai permessi;
- cambio password;
- audit cambio password;
- permessi caricati al login.

## Test consigliato

```powershell
dotnet clean AccyourateEnterpriseX.sln
dotnet restore AccyourateEnterpriseX.sln
dotnet build AccyourateEnterpriseX.sln
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

## Test funzionale

1. Accedi con `admin / admin123`.
2. Apri Gestione Utenti.
3. Crea un utente Operatore.
4. Chiudi e accedi con il nuovo utente.
5. Verifica che il menu sia ridotto.
6. Prova Cambio Password.


# Release Process 1.1

Da questa versione il progetto segue il ciclo:

```text
Developer Edition
↓
Release Candidate
↓
Stable
```

## Test completo

```text
scripts\00_test_full.bat
```

## Promozione a RC

Solo dopo test positivo:

```text
scripts\01_promote_to_rc.bat
```

## Promozione a Stable

Solo dopo collaudo RC positivo:

```text
scripts\02_promote_to_stable.bat
```

## Checklist

Aprire:

```text
docs\test\CHECKLIST_COLLAUDO.md
```


# Developer Edition 1.2 - Navigation & CRUD Foundation

Aggiunge:
- navigazione più ordinata;
- breadcrumb;
- registrazione moduli;
- framework CRUD placeholder;
- azioni standard per i moduli.

## Test

```powershell
dotnet clean AccyourateEnterpriseX.sln
dotnet restore AccyourateEnterpriseX.sln
dotnet build AccyourateEnterpriseX.sln
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```

## Test funzionale

1. Accedi come admin.
2. Apri Persone.
3. Verifica finestra CRUD predisposta.
4. Apri Asset IT.
5. Apri Dispositivi Medici.
6. Verifica Gestione Utenti, Diagnostica e Cambio Password.


# Developer Edition 1.3 - Persone/Dipendenti

Aggiunge il primo modulo reale:

- tabella `employees`;
- creazione dipendente;
- elenco dipendenti;
- ricerca;
- archivia/ripristina;
- audit.

## Test funzionale

1. Accedi come admin.
2. Apri **Persone**.
3. Crea un dipendente:
   - Matricola: DIP001
   - Nome: Mario
   - Cognome: Rossi
   - Reparto: Produzione
   - Mansione: Tecnico
4. Verifica che compaia nell'elenco.
5. Cerca `Mario` o `DIP001`.
6. Archivia il dipendente.
7. Spunta `Includi archiviati`.
8. Ripristina il dipendente.
9. Apri Diagnostica e controlla audit.


# Developer Edition 1.4 - Persone Completo Base

Aggiunge al modulo Persone:

- modifica dipendente;
- scheda dettaglio;
- export CSV compatibile Excel;
- generazione scheda dipendente in TXT;
- audit aggiornamenti ed export.

## Test funzionale

1. Apri Persone.
2. Crea o seleziona un dipendente.
3. Premi Apri.
4. Premi Modifica e cambia un campo.
5. Premi Esporta CSV.
6. Premi Scheda.
7. Verifica i file in:

```text
C:\ProgramData\Accyourate Enterprise X\exports
```


# Developer Edition 2.0 - IT Asset Management

Aggiunge il modulo reale Asset IT:

- tabella `assets`;
- creazione asset;
- categorie hardware;
- assegnazione a dipendente;
- rientro asset;
- ricerca;
- archivia/ripristina;
- export CSV;
- payload QR in JSON;
- audit.

## Test funzionale

1. Crea almeno un dipendente nel modulo Persone.
2. Apri Asset IT.
3. Crea un asset:
   - Codice: IT001
   - Categoria: Notebook
   - Marca: Dell
   - Modello: Latitude
   - Seriale: SN001
   - OS: Windows 11
   - Assegnato a: dipendente creato
4. Verifica che compaia in elenco.
5. Cerca per codice o seriale.
6. Premi QR.
7. Premi Esporta CSV.
8. Premi Rientro.
9. Archivia e ripristina.


# Developer Edition 3.0 - Workflow Foundation

Aggiunge:

- tabella `workflow_events`;
- finestra Workflow & Cronologia Eventi;
- cambio stato Asset IT verso Manutenzione;
- registrazione eventi workflow;
- base per futuri Dispositivi Medici, Capi Tessili e Control Unit.

## Test funzionale

1. Accedi come admin.
2. Apri Asset IT.
3. Crea o usa un asset esistente.
4. Premi `Manut.`.
5. Apri `Workflow`.
6. Verifica che compaia un evento `STATUS_CHANGED`.
7. Cerca per codice asset o per `Manutenzione`.


# Developer Edition 3.1 - Project Infrastructure

Aggiunge:

- struttura cartelle professionale;
- `.gitignore`;
- database versioning;
- tabella configurazioni;
- backup manuale database;
- finestra Infrastruttura;
- documentazione Git workflow;
- checklist release;
- base installer Windows.

## Test funzionale

1. Accedi come admin.
2. Apri `Infrastruttura`.
3. Verifica versioni database.
4. Verifica configurazioni.
5. Premi `Crea Backup Database`.
6. Controlla che il backup compaia in lista.
7. Verifica il file in:

```text
C:\ProgramData\Accyourate Enterprise X\backups
```


# RC 4.0 - Medical Device Foundation

Aggiunge:

- modulo Medical Device Suite;
- anagrafica Dispositivi Medici;
- Control Unit;
- Capi Tessili;
- Digital Twin con timeline;
- workflow verso Collaudato;
- RFID/QR logico;
- export CSV.

## Test funzionale

1. Accedi come admin.
2. Apri `Dispositivi Medici`.
3. Crea un dispositivo:
   - Codice: MED001
   - Tipo: Control Unit
   - Modello: CU-01
   - Seriale: SN001
   - Lotto: LOT001
   - RFID: RFID001
4. Premi `CU` e salva dati firmware/MAC.
5. Crea un secondo dispositivo di tipo Top o T-Shirt.
6. Premi `Capo` e salva dati tessili.
7. Premi `Collaudo`.
8. Apri `Twin` e verifica la timeline.
9. Apri `Workflow` e cerca MED001.


# RC 4.0 Fix 1

Correzione build:

```text
CS1061: SqliteConnection non contiene LastInsertRowId
```

La proprietà è stata sostituita con:

```sql
SELECT last_insert_rowid()
```

## Test

```powershell
dotnet clean AccyourateEnterpriseX.sln
dotnet restore AccyourateEnterpriseX.sln
dotnet build AccyourateEnterpriseX.sln
dotnet run --project src\Accyourate.App\Accyourate.App.csproj
```













































