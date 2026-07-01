# Accyourate Enterprise X - Blueprint v1.0

## Scopo del documento

Questo Blueprint è il documento guida ufficiale di Accyourate Enterprise X.

Definisce:

- visione del prodotto;
- principi architetturali;
- roadmap;
- moduli funzionali;
- standard di sviluppo;
- regole di qualità;
- Product Constitution;
- direzione futura.

Ogni nuova funzionalità, refactoring o decisione architetturale dovrà essere coerente con questo documento.

---

# Parte I - Visione

## Missione

Accyourate Enterprise X nasce per diventare una piattaforma enterprise modulare, desktop-first, capace di gestire dati, asset, processi, persone, dispositivi, AI e in futuro anche componenti medicali e wearable.

## Problemi che risolve

- Frammentazione dei dati aziendali.
- Inventari gestiti con file Excel non collegati tra loro.
- Mancanza di tracciabilità su asset, dipendenti, sedi e fornitori.
- Assenza di una visione integrata tra dati operativi, AI e Digital Twin.
- Difficoltà nel far crescere un gestionale senza accumulare codice duplicato.

## Utenti target

- Piccole e medie imprese.
- Reparti IT.
- Aziende con asset distribuiti.
- Centri estetici e strutture operative.
- Realtà medicali e R&D.
- Organizzazioni che vogliono evolvere da Excel a una piattaforma gestionale.

## Obiettivi a 3-5 anni

- Piattaforma desktop stabile.
- Moduli enterprise completi.
- AI operativa integrata.
- Medical Platform con wearable e monitoraggio ECG.
- Versione Web e Mobile.
- API e integrazioni esterne.
- Marketplace o sistema plugin per moduli aggiuntivi.

---

# Parte II - Architettura

## Principi architetturali

Accyourate deve evolvere seguendo questi principi:

1. Modularità.
2. Separazione tra UI, dominio e infrastruttura.
3. Domain-Driven Design.
4. Dependency Injection.
5. Repository Pattern.
6. Testabilità.
7. Documentazione viva.
8. Compatibilità Windows e macOS.
9. Estensibilità futura verso Web, Mobile e Cloud.

## Architettura target

```text
Accyourate.sln
│
├── src
│   ├── Accyourate.App
│   ├── Accyourate.Core
│   ├── Accyourate.Domain
│   ├── Accyourate.Infrastructure
│   ├── Accyourate.Modules.Asset
│   ├── Accyourate.Modules.MasterData
│   ├── Accyourate.Modules.AI
│   ├── Accyourate.Modules.Medical
│   └── Accyourate.Shared
│
├── tests
│   ├── Accyourate.Core.Tests
│   ├── Accyourate.Asset.Tests
│   └── Accyourate.MasterData.Tests
│
└── docs
```

## Stato attuale

Il progetto è ancora concentrato principalmente in `Accyourate.App`, ma la direzione architetturale è la migrazione graduale verso domini separati.

La regola è:

> Nessun grande refactoring massivo. Ogni migrazione deve mantenere il progetto compilabile.

---

# Parte III - Piattaforma

## Workspace

La Workspace è il centro dell'applicazione.

Ogni modulo principale deve aprirsi come tab interna, non come finestra esterna.

Moduli già integrati:

- Dashboard
- Digital Twin
- AI Assistant
- Action Engine
- Universal Command Bar
- Asset Management
- Anagrafica Aziendale

## UI Framework

Il framework UI condiviso contiene componenti riutilizzabili:

- EnterpriseKpiCard
- EnterpriseToolbar
- EnterpriseSearchBar
- EnterpriseDetailsPanel
- EnterpriseStatusBadge
- EnterpriseSectionHeader
- EnterpriseCard
- EnterpriseDialogBase

Obiettivo:

> Ogni nuova schermata deve usare progressivamente il UI Framework condiviso.

## Generic CRUD Engine

Obiettivo futuro:

- generare schermate CRUD coerenti;
- centralizzare validazioni;
- ridurre duplicazione;
- rendere più rapida la creazione di nuovi moduli.

## Audit Log

Ogni modifica significativa dovrà registrare:

- utente;
- data;
- operazione;
- entità coinvolta;
- valore precedente;
- nuovo valore.

## Notification Center

Gestirà:

- garanzie in scadenza;
- manutenzioni;
- licenze;
- promemoria;
- notifiche operative.

## Feature Flags

Ogni modulo potrà essere abilitato o disabilitato.

Esempio:

```text
Asset Management: enabled
Master Data: enabled
Medical: disabled
Warehouse: disabled
HR: disabled
```

---

# Parte IV - Moduli

## Asset Management

### Finalità

Gestire il ciclo di vita dei beni aziendali.

### Entità principali

- Asset
- Employee
- Assignment
- Maintenance
- Document
- Credential

### Funzionalità già presenti

- lista asset;
- ricerca;
- filtri;
- KPI;
- dettagli;
- creazione;
- modifica;
- eliminazione;
- validazione;
- controllo duplicati.

### Evoluzioni previste

- assegnazioni reali;
- collegamento a dipendenti;
- manutenzioni;
- documenti;
- QR code;
- import/export Excel;
- AI query.

---

## Master Data

### Finalità

Gestire le anagrafiche comuni dell'organizzazione.

### Entità

- Company
- Site
- Department
- Employee
- Supplier

### Funzionalità già presenti

- database;
- servizio;
- dati demo;
- workspace module;
- KPI;
- navigazione sezioni;
- ricerca;
- pannello dettagli.

### Evoluzioni previste

- CRUD completo;
- collegamento con Asset Management;
- gestione multi-azienda;
- ruoli e permessi.

---

## AI Platform

### Finalità

Rendere Accyourate interrogabile e operativo tramite linguaggio naturale.

### Obiettivi futuri

- query sui dati enterprise;
- suggerimenti;
- report automatici;
- automazioni;
- assistente operativo.

Esempi:

- "Mostrami tutti i notebook Lenovo."
- "Quali garanzie scadono entro 60 giorni?"
- "Quali asset sono assegnati al reparto IT?"

---

## Medical Platform

### Visione

Il modulo Medical nascerà per integrare dispositivi wearable e dati biometrici.

### Possibili funzionalità

- monitoraggio ECG;
- frequenza cardiaca;
- sensori tessili;
- storico misurazioni;
- allarmi;
- dashboard cliniche;
- Digital Twin sanitario.

Il progetto del corpetto intelligente rientra in questa direzione.

---

## Moduli futuri

- Warehouse
- Service Desk
- HR
- CRM
- Procurement
- Documents
- Analytics
- Reports
- Mobile/Web

---

# Parte V - Roadmap

## Versione 13 - Platform

Obiettivo: consolidamento piattaforma.

- Enterprise UI Framework
- UI Framework Adoption
- Generic CRUD Engine
- Audit Log
- Notification Center
- Feature Flags
- Test automatici

## Versione 14 - Business Modules

Obiettivo: moduli operativi.

- Warehouse
- Service Desk
- Maintenance Planning
- License Management
- Documents

## Versione 15 - Enterprise Intelligence

Obiettivo: collegare dati e moduli.

- relazioni tra dipendenti, asset, sedi, reparti e fornitori;
- viste trasversali;
- KPI avanzati;
- report.

## Versione 16 - AI Enterprise

Obiettivo: AI realmente operativa.

- interrogazioni sui dati;
- suggerimenti;
- automazioni;
- report generati.

## Versione 17 - Medical

Obiettivo: piattaforma medicale.

- wearable;
- ECG;
- dati biometrici;
- allarmi;
- dashboard cliniche.

## Versione 18 - Cloud, Web e Mobile

Obiettivo: estensione multipiattaforma.

- API;
- web app;
- mobile app;
- sincronizzazione cloud;
- notifiche push.

## Versione 2.0

Obiettivo: piattaforma enterprise completa.

- moduli principali;
- AI integrata;
- ruoli e permessi;
- audit;
- API;
- cloud readiness;
- documentazione completa.

---

# Parte VI - Product Constitution

1. Ogni funzionalità deve essere modulare.
2. Ogni modulo deve avere un'identità chiara.
3. Nessun modulo deve accedere direttamente ai dati di un altro modulo senza interfaccia.
4. Ogni nuova schermata deve usare il UI Framework condiviso.
5. Ogni modifica significativa deve aggiornare documentazione e changelog.
6. Ogni release stabile deve avere tag Git.
7. `main` deve contenere solo versioni stabili.
8. `develop` deve contenere integrazione validata.
9. Ogni feature deve nascere su branch dedicato.
10. Ogni sprint deve avere checklist.
11. Ogni nuova architettura deve avere ADR.
12. La compatibilità Windows/macOS resta requisito strategico.
13. Il codice deve essere scritto pensando a Web/Mobile futuri.
14. Le password e i segreti non devono essere salvati in chiaro.
15. Ogni funzionalità premium deve poter diventare feature flag.

---

# Parte VII - Standard di sviluppo

## Branch

```text
main
develop
feature/*
fix/*
release/*
```

## Commit

Prefissi consigliati:

- UI
- CORE
- DATA
- ASSET
- MASTER
- AI
- MEDICAL
- FIX
- DOC
- TEST

Esempi:

```text
UI-002: Adopt EnterpriseKpiCard in Asset Management
DOC-006: Add project governance and Git workflow
FIX-001: Resolve Asset dialog crash
```

## Quality Gate

Ogni sprint deve verificare:

- build locale;
- GitHub Actions;
- smoke test;
- test funzionale;
- documentazione;
- commit;
- push.

## Release

Ogni release stabile deve avere:

- merge su main;
- tag Git;
- changelog;
- release notes;
- checklist completata.

---

# Parte VIII - Backlog strategico

## Alta priorità

- UI Framework adoption completa.
- Master Data CRUD.
- Asset-MasterData integration.
- Test automatici.
- Generic CRUD Engine.
- Audit Log.

## Media priorità

- Notification Center.
- Feature Flags.
- Import/export Excel.
- QR Code asset.
- Service Desk.
- Document Management.

## Bassa priorità

- Cloud sync.
- Mobile app.
- Plugin marketplace.
- API pubbliche.
- Integrazioni ERP esterne.

---

# Conclusione

Accyourate Enterprise X deve evolvere come piattaforma modulare, coerente e documentata.

Ogni sprint deve contribuire alla visione complessiva, non solo aggiungere codice.

Questo Blueprint è la bussola del progetto.
