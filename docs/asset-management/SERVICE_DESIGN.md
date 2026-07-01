# Asset Management - Service Design

## Servizi previsti

### IAssetService

Responsabilità:

- elenco asset;
- ricerca;
- filtri;
- creazione;
- modifica;
- cambio stato;
- lettura scheda asset.

Metodi ipotetici:

```csharp
IReadOnlyList<Asset> GetAssets();
Asset? GetAssetById(int id);
Asset? GetAssetByCode(string code);
IReadOnlyList<Asset> SearchAssets(string query);
void CreateAsset(Asset asset);
void UpdateAsset(Asset asset);
void ChangeStatus(int assetId, string status);
```

---

### IEmployeeService

Responsabilità:

- elenco dipendenti;
- ricerca;
- scheda dipendente;
- stato attivo/non attivo.

---

### IAssetAssignmentService

Responsabilità:

- assegnazione asset;
- restituzione asset;
- storico assegnazioni;
- asset assegnati a una persona.

---

### IMaintenanceService

Responsabilità:

- apertura intervento;
- chiusura intervento;
- storico interventi;
- KPI interventi aperti.

---

### IAssetDocumentService

Responsabilità:

- documenti collegati;
- upload;
- ricerca;
- apertura file.

---

## Architettura suggerita

```text
AssetManagementView
        │
        ▼
AssetService
        │
        ▼
DatabaseService
        │
        ▼
SQLite
```

## Integrazione Workspace

Il modulo sarà registrato come:

```text
asset-management
```

Titolo:

```text
Asset Management
```

Icona:

```text
IT
```
