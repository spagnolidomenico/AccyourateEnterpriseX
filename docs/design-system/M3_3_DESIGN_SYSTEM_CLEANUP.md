# M3.3 — Design System Cleanup & Stabilization

## Risultato

Lo sprint consolida la foundation introdotta in M3.1 e rimuove l'uso del layer legacy dai componenti del Design System.

## Modifiche principali

- introdotto `AxThemeManager` come coordinatore centrale del tema applicativo;
- collegato il tema ad `Application.RequestedThemeVariant` di Avalonia;
- predisposti i temi Light e Dark tramite `UiThemeMode` e `AxThemePalette`;
- migrati `AxButtons`, `AxCards`, `AxBadges`, `AxTypography`, `AxLayout` e la showcase ai token `Ax*`;
- eliminati i riferimenti a `AccyourateDesignTokens` dal codice applicativo, mantenendo il file solo come compatibilità temporanea;
- integrato il cambio tema dell'Enterprise Workspace con il nuovo Theme Manager;
- esteso lo script di controllo del Design System con le verifiche M3.3.

## Compatibilità

`AccyourateDesignTokens` rimane presente per evitare rotture a eventuali estensioni esterne, ma non è più utilizzato dal progetto applicativo. Potrà essere eliminato in uno sprint successivo dopo la verifica delle integrazioni.

## Verifica locale

```powershell
pwsh ./scripts/test-m3-design-system-foundation.ps1
dotnet restore
dotnet build
```

Il cambio tema può essere verificato dall'Enterprise Workspace tramite il comando tema già esistente.
