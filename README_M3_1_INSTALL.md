# Accyourate Enterprise X — M3.1 Design System Foundation

## Contenuto

- token colore primitivi;
- token colore semantici;
- scala spacing e radius;
- scala tipografica;
- palette light/dark centralizzata;
- compatibilità con `UiTokens`;
- deprecazione controllata di `AccyourateDesignTokens`;
- test PowerShell dedicato;
- documentazione architetturale.

## Installazione

I file sono già integrati nel progetto. Il progetto SDK include automaticamente i nuovi file `.cs`.

## Test

```powershell
.\scripts\test-m3-design-system-foundation.ps1
```

## Nota branch

Il progetto ricevuto risultava sul branch `feature/13.0.1-enterprise-ui-framework`. Prima del commit verificare il branch desiderato con:

```powershell
git branch --show-current
```

Se necessario, creare il branch M3 preservando le modifiche correnti:

```powershell
git switch -c feature/m3-design-system
```
