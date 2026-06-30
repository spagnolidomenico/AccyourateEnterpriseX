# Accyourate Enterprise X - Architettura modulare ufficiale

## Core

```text
Core
├── Security
├── Workflow
├── Audit
├── Configuration
└── Notifications
```

## Medical

```text
Medical
├── Devices
├── Production
├── Quality
├── Warehouse
├── Laundry
├── Maintenance
└── Digital Twin
```

## IT

```text
IT
├── Assets
├── Inventory
└── Licenses
```

## HR

```text
HR
├── People
├── Roles
└── Training
```

## Altri moduli

- Documents
- Reports
- Administration

## Regola

Ogni nuovo modulo deve usare:
- permessi centralizzati;
- audit;
- workflow quando applicabile;
- componenti UI condivisi;
- configurazione centralizzata.
