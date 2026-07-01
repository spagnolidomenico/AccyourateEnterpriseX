# 13.0.1 RC2 - EnterpriseToolbar Build Fix

## Obiettivo

Correggere gli errori di compilazione introdotti in `EnterpriseToolbar`.

## Problema

Il metodo interno `Button()` riceveva parametri non coerenti con le chiamate:

```text
CS1503: cannot convert from string to Avalonia.Media.IBrush
```

## Correzione

`Button()` ora accetta:

- `backgroundToken` come `string`;
- `foreground` come `IBrush`.

Il background viene convertito internamente con:

```csharp
UiTokens.Brush(backgroundToken)
```

## Criteri di accettazione

- Smoke test superato.
- GitHub Actions verde.
- Nessuna regressione visiva.
