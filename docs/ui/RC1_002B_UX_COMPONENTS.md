# RC1-002B - UX Components

## Obiettivo

Introdurre componenti UX condivisi per dialoghi, messaggi, loading e feedback utente.

## Componenti introdotti

- `AxMessageKind`
- `AxDialogResult`
- `AxDialogService`
- `AxStatusBanner`
- `AxLoadingOverlay`
- `AxSnackbar`
- `AxUxGuidelines`

## Regole di utilizzo

### Dialoghi

Usare `AxDialogService.ConfirmAsync` per operazioni distruttive:

```csharp
var result = await AxDialogService.ConfirmAsync(owner, "Eliminare?", "Operazione irreversibile.", "Elimina", "Annulla", AxMessageKind.Error);
```

### Loading

Usare `AxLoadingOverlay.Create("Backup in corso...")` per operazioni lunghe.

### Banner

Usare `AxStatusBanner.Create("Operazione completata", AxMessageKind.Success)` per messaggi persistenti.

### Snackbar

Usare `AxSnackbar` per feedback brevi non bloccanti.

## Prossimo sprint

`RC1-002C - Workspace Enhancements`
