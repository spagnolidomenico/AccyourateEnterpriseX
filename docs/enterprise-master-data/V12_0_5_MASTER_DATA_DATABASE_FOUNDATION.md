# Sprint 12.0.5 - Master Data Database Foundation

## Obiettivo

Introdurre la base dati del modulo Anagrafica Aziendale.

## Componenti introdotti

### Modelli

- `Company`
- `Site`
- `Department`
- `EmployeeMasterData`
- `Supplier`

### Servizi

- `MasterDataService`
- `MasterDataBootstrap`

### Database

Database SQLite locale dedicato:

```text
%APPDATA%/AccyourateEnterpriseX/accyourate-master-data.db
```

Tabelle:

- `Companies`
- `Sites`
- `Departments`
- `Employees`
- `Suppliers`

## Dati demo

Alla prima inizializzazione vengono creati:

- Accyourate Group;
- sede principale;
- sede operativa;
- reparti IT, Amministrazione, Operations, Medical R&D;
- dipendenti demo;
- fornitori demo.

## Cosa NON cambia

- Non viene ancora introdotta la schermata Anagrafica Aziendale.
- Asset Management non è ancora collegato a Master Data.
- Le assegnazioni asset saranno introdotte dopo la UI Master Data.

## Criteri di accettazione

- Build locale superata.
- GitHub Actions verde.
- Nessuna regressione sui moduli principali.
