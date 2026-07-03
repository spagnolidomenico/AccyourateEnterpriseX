# Module Standard

Ogni nuovo modulo deve seguire questa struttura:

```text
ModuleName
├── Views
├── ViewModels
├── Models
├── Services
├── Repositories
├── Commands
├── Validators
├── Permissions
├── Notifications
├── Audit
├── Documentation
└── Tests
```

Le View non devono contenere logica business complessa, query SQL o accesso diretto ai database.
