# RC1-002A - Enterprise Design System

## Obiettivo

Introdurre componenti UI condivisi per rendere coerente l'interfaccia di Accyourate Enterprise X.

## Componenti introdotti

- `AxSpacing`
- `AxTypography`
- `AxButton`
- `AxButtonKind`
- `AxCard`
- `AxSection`
- `AxEmptyState`
- `AxStatusMessage`
- `AxPageHeader`
- `AxStatusBar`

## Regole

- Margine pagina: 24 px
- Padding card: 18 px
- Spaziatura elementi: 12 px
- Micro spacing: 8 px
- Titolo pagina: 34 px
- Titolo sezione: 22 px

## Esempi

```csharp
AxButton.Create("Salva", Save, AxButtonKind.Primary);
AxCard.Create(content);
AxPageHeader.Create("Titolo", "Descrizione", actions);
```

## Prossimo sprint

`RC1-002B - UX Components`
