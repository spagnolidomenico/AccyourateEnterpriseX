# Architecture Overview

```text
Accyourate Enterprise X
├── Core Platform
│   ├── Notifications
│   ├── Audit
│   ├── Logging
│   ├── Search
│   ├── Permissions
│   ├── Workflow
│   └── Configuration
├── UI Framework
├── Business Modules
└── Infrastructure
```

I moduli non devono dipendere direttamente da altri moduli. Devono comunicare tramite Platform Services, repository, eventi, notifiche, audit e workflow.
