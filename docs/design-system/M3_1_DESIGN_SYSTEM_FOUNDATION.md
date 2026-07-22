# M3.1 — Design System Foundation

## Obiettivo

Creare una sola fonte autorevole per i token visivi di Accyourate Enterprise X, mantenendo compatibilità con le schermate già sviluppate.

## Architettura canonica

La foundation è collocata in:

`src/Accyourate.App/UIFramework/Foundation`

### Token primitivi

- `AxColorTokens`: palette tecnica, senza significato funzionale.

### Token semantici

- `AxSemanticTokens`: colori descritti per intenzione (`Surface`, `TextPrimary`, `Danger`, ecc.).
- `AxThemePalette`: selezione coerente dei token light/dark.

### Token strutturali

- `AxLayoutTokens`: spacing, radius, altezze dei controlli e larghezze standard.
- `AxTypographyTokens`: famiglia e scala tipografica.

## Compatibilità

`UiTokens` resta disponibile come facciata per il codice esistente, ma delega alla nuova foundation.

`AccyourateDesignTokens` è mantenuto come layer legacy ed è marcato `Obsolete`. Non deve essere usato nei nuovi componenti.

## Regole M3

1. Non inserire nuovi colori esadecimali direttamente nei componenti.
2. Preferire sempre token semantici ai token primitivi.
3. Usare `AxLayoutTokens` per spacing, radius e dimensioni ricorrenti.
4. Usare `AxTypographyTokens` per la scala tipografica.
5. Non creare nuove classi tema parallele.
6. Migrare gradualmente i moduli esistenti; nessuna riscrittura massiva nello sprint foundation.

## Esempio

```csharp
using Accyourate.App.UIFramework.Foundation;
using Accyourate.App.UIFramework.Tokens;

Background = UiTokens.Brush(AxSemanticTokens.Surface);
Padding = new Thickness(AxLayoutTokens.Space4);
CornerRadius = new CornerRadius(AxLayoutTokens.RadiusMedium);
```

## Verifica

Da PowerShell:

```powershell
.\scripts\test-m3-design-system-foundation.ps1
```

Per eseguire solo i controlli statici:

```powershell
.\scripts\test-m3-design-system-foundation.ps1 -SkipBuild
```

## Strategia di migrazione

- M3.1: foundation e compatibility facade.
- M3.2: component primitives e stati interattivi.
- M3.3: applicazione a una schermata pilota.
- M3.4: progressiva rimozione delle dipendenze legacy.
