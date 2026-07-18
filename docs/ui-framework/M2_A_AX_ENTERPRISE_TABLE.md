# M2-A - AxEnterpriseTable Foundation

## Obiettivo

Introdurre una tabella enterprise riutilizzabile per Asset, HR, Documenti e futuri moduli.

## Componenti

- `AxColumnAlignment`
- `AxEnterpriseColumn<T>`
- `AxEnterpriseTable<T>`

## Caratteristiche

- Intestazione e celle condividono la stessa struttura di colonne.
- Larghezze minime e preferite.
- Scroll orizzontale solo quando necessario.
- Righe con altezza uniforme.
- Selezione uniforme.
- Celle testuali o personalizzate.
- Supporto badge e controlli complessi tramite `CellFactory`.

## Esempio

```csharp
var table = new AxEnterpriseTable<AssetRow>();

table.ConfigureColumns(new[]
{
    new AxEnterpriseColumn<AssetRow>
    {
        Id = "code",
        Header = "Codice",
        MinWidth = 130,
        Width = 150,
        TextSelector = x => x.Code
    },
    new AxEnterpriseColumn<AssetRow>
    {
        Id = "status",
        Header = "Stato",
        MinWidth = 150,
        Width = 170,
        Alignment = AxColumnAlignment.Center,
        CellFactory = x => AxStatusBadge.FromStatus(x.Status)
    }
});
```

## Prossimo passo

Integrare progressivamente `AxEnterpriseTable<T>` nei moduli:

1. Asset Management
2. Human Resources
3. Document Center
