# Accyourate Enterprise X - Struttura progetto

## Obiettivo

Separare progressivamente il progetto in aree chiare:

```text
src/
 └── Accyourate.App/
     ├── Core
     ├── Infrastructure
     ├── Modules
     │   ├── People
     │   ├── Assets
     │   └── MedicalDevices
     └── Shared
```

Questa versione mantiene ancora il progetto singolo per evitare rotture, ma prepara le cartelle per la separazione futura.
