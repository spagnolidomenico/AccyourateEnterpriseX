# Accyourate Enterprise X - 14.0.0 Core Platform Refactoring Blueprint

## Obiettivo

Preparare Accyourate Enterprise X a una vera architettura enterprise, risolvendo la duplicazione dei domini e creando una base comune per Asset Management, Master Data, AI, Medical e moduli futuri.

## Problema individuato

Durante il tentativo di collegare Dipendenti e Asset è emersa una duplicazione:

```text
Asset Management
  └── Employees

Master Data
  └── Employees
```

Questa duplicazione genera problemi di consistenza e vincoli SQLite, come:

```text
FOREIGN KEY constraint failed
```

## Decisione

Prima di continuare con nuove funzionalità operative, il progetto deve introdurre una base comune:

```text
Core Platform
Domain Model
Repository Layer
Shared Services
Dependency Injection
```

## Principio guida

Una sola entità deve avere una sola fonte di verità.

Esempio:

```text
Employee = definito una sola volta
Asset = definito una sola volta
Assignment = relazione tra Employee e Asset
```

---

# Architettura target

```text
src
├── Accyourate.App
├── Accyourate.Core
├── Accyourate.Domain
├── Accyourate.Infrastructure
├── Accyourate.Modules.Asset
├── Accyourate.Modules.MasterData
└── Accyourate.Shared
```

## Accyourate.Domain

Contiene le entità principali:

```text
Employees
Assets
Assignments
Companies
Sites
Departments
Suppliers
```

## Accyourate.Infrastructure

Contiene:

```text
SQLite
Repositories
Migrations
DatabaseContext
```

## Accyourate.Core

Contiene:

```text
Service registry
Dependency injection
Events
Feature flags
Audit contracts
```

---

# Modello dati target

## Employee

```text
Id
FullName
Email
Phone
Role
DepartmentId
SiteId
IsActive
Notes
```

## Asset

```text
Id
AssetCode
Category
Manufacturer
Model
SerialNumber
AssetTag
Status
WarrantyEndDate
Notes
```

## AssetAssignment

```text
Id
AssetId
EmployeeId
AssignedAt
ReturnedAt
Status
Notes
```

## Regola importante

`AssetAssignment.EmployeeId` deve puntare all'unica tabella dipendenti valida.

---

# Roadmap di refactoring

## 14.0.1 - Architecture Baseline

- Creare cartelle/progetti target.
- Aggiungere documentazione architetturale.
- Nessuna modifica funzionale.

## 14.0.2 - Domain Contracts

- Introdurre entità condivise.
- Definire interfacce repository.
- Nessuna migrazione database ancora.

## 14.0.3 - Infrastructure Foundation

- Creare repository SQLite.
- Preparare migrazioni.
- Definire database context.

## 14.0.4 - Employee Unification

- Rendere Master Data la fonte unica dei dipendenti.
- Rimuovere gradualmente Employees da Asset Management.
- Mappare dati esistenti.

## 14.0.5 - Asset Assignment Rebuild

- Ricostruire Employee ↔ Asset Assignment sul modello unificato.
- Evitare foreign key errate.
- Visualizzare asset assegnati e assegnatario.

## 14.0.6 - Tests

- Aggiungere test per EmployeeRepository.
- Aggiungere test per AssetRepository.
- Aggiungere test per AssetAssignment.

---

# Regole operative

1. Nessun grande refactoring in un'unica patch.
2. Ogni sprint deve compilare.
3. Ogni migrazione database deve essere reversibile o sicura.
4. Prima si introduce il nuovo modello, poi si collega la UI.
5. Nessuna nuova feature deve creare un secondo modello duplicato.
6. Ogni decisione architetturale deve avere ADR.

---

# Strategia di rischio

## Rischi

- rottura database esistenti;
- perdita dati demo;
- vincoli FK errati;
- regressioni su Asset Management;
- regressioni su Master Data.

## Mitigazioni

- patch piccole;
- backup database;
- migrazioni incrementali;
- test manuali dopo ogni patch;
- nessuna eliminazione immediata del vecchio schema.

---

# Criteri di successo

Il refactoring sarà completato quando:

- esiste una sola anagrafica dipendenti;
- Asset Management usa i dipendenti di Master Data;
- le assegnazioni Asset ↔ Dipendente funzionano;
- non esistono più errori FK;
- il codice è predisposto per test automatici;
- l'architettura è documentata.
