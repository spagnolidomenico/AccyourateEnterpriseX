# Sprint 12.0.1 - Asset Database Foundation

## Obiettivo

Introdurre il livello dati base del modulo Enterprise Asset Management.

## Componenti introdotti

### Modelli

- `Asset`
- `Employee`
- `AssetAssignment`
- `MaintenanceTicket`
- `AssetDocument`
- `AssetCredential`

### Servizi

- `AssetService`
- `AssetManagementBootstrap`

### Database

Database locale SQLite:

```text
%APPDATA%/AccyourateEnterpriseX/accyourate-assets.db
```

Tabelle:

- `Assets`
- `Employees`
- `AssetAssignments`
- `MaintenanceTickets`
- `AssetDocuments`
- `AssetCredentials`

## Dati demo

Alla prima inizializzazione vengono creati asset demo:

- PC-001
- NB-001
- MAC-001
- PRN-001
- PH-001

## Cosa NON cambia

- Non viene ancora introdotta la schermata Asset Management.
- Non viene ancora registrato il modulo nella Workspace.
- Non vengono ancora gestite assegnazioni da UI.
- Le credenziali non sono ancora cifrate.

## Criteri di accettazione

- Build locale superata.
- GitHub Actions verde.
- Nessuna regressione su Workspace.
- Nessuna regressione sui moduli principali.
