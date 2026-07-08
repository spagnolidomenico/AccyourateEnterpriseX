# EP-003A - Design System v2 Foundation

## Obiettivo

Centralizzare i componenti UI principali di Accyourate Enterprise X.

## Componenti introdotti

- `AxKpiCard`
- `AxToolbar`
- `AxSearchBox`
- `AxStatusBadge`
- `AxInfoPanel`
- `AxTimeline`
- `AxDashboardWidget`

## Regola

Le nuove schermate non devono creare pulsanti, card, badge, pannelli o toolbar personalizzati quando esiste un componente Ax equivalente.

## Uso consigliato

### KPI

```csharp
AxKpiCard.Create("💻", "Asset Totali", "312", "Apri Asset", OpenAsset);
```

### Badge

```csharp
AxStatusBadge.FromStatus("Disponibile");
```

### Toolbar

```csharp
var toolbar = new AxToolbar()
    .AddLeft(AxSearchBox.Create("Cerca..."))
    .AddRight(AxButton.Create("Nuovo", Create, AxButtonKind.Primary));
```

### Info Panel

```csharp
var panel = new AxInfoPanel("Scheda tecnica")
    .AddItem("Categoria", "Notebook", "💻")
    .AddItem("Stato", "Disponibile", "🟢")
    .ToCard();
```

## Prossimo passo

Applicare il Design System v2 ai moduli principali:

1. Asset Management
2. Human Resources
3. Document Center
4. Backup Center
5. Update Center
