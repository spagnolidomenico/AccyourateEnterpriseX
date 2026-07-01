# 13.0.5.1 - EnterpriseToolbar Click Hotfix

## Problema

Nel modulo Asset Management, premendo `Aggiorna` dalla toolbar, l'app poteva tornare alla Workspace/Home.

## Causa probabile

Il click del pulsante nella `EnterpriseToolbar` poteva propagarsi al contenitore padre.

## Correzione

Il click dei pulsanti `EnterpriseToolbar` ora marca l'evento come gestito:

```csharp
e.Handled = true;
```

## Test

- Asset Management resta aperto dopo `Aggiorna`.
- `+ Nuovo` apre il dialog.
- Nessuna regressione sui moduli principali.
