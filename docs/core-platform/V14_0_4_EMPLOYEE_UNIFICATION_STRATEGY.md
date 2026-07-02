# 14.0.4 - Employee Unification Strategy

## Principio

Master Data diventa la fonte ufficiale dei dipendenti.

## Stato attuale

```text
MasterDataService
  accyourate-master-data.db
  Employees: FullName, Email, Phone, Role, DepartmentId, SiteId, IsActive, Notes

AssetService
  accyourate-assets.db
  Employees: FullName, Email, Department, Role, Site, IsActive
```

## Vincolo attuale

`AssetAssignments.EmployeeId` punta alla tabella `Employees` dentro `accyourate-assets.db`.

## Strategia incrementale

### Fase 1 - Preflight

- trovare database reali;
- verificare schema;
- verificare dati esistenti;
- nessuna modifica dati.

### Fase 2 - Bridge

Creare un bridge controllato:

```text
Master Data Employees
        ↓
Asset Employees mirror
        ↓
AssetAssignments.EmployeeId
```

La tabella Asset `Employees` resta temporaneamente come mirror tecnico per rispettare le FK esistenti.

### Fase 3 - Sync

Sincronizzare i dipendenti Master Data nel database Asset.

Regole:

- match primario: email;
- fallback: FullName;
- non cancellare dipendenti legacy;
- disattivare, non eliminare;
- mantenere Id Asset Employees per FK.

### Fase 4 - Relationship Engine

Solo dopo la sincronizzazione:

- assegnazione Asset ↔ Dipendente;
- restituzione;
- storico;
- trasferimenti.

### Fase 5 - Schema futuro

In futuro si potrà valutare un database unico o una migrazione più profonda.
