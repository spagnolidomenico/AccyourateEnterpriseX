# Workspace 2.0 - Test Plan

## Smoke Test

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\test-smoke.ps1
```

## Test funzionale base

- [ ] App avviata
- [ ] Login funzionante
- [ ] Workspace aperta
- [ ] Dashboard aperta come tab
- [ ] Digital Twin aperto come tab
- [ ] AI Assistant aperto come tab
- [ ] Nessuna duplicazione tab
- [ ] Chiusura tab funzionante
- [ ] Universal Command Bar funzionante
- [ ] Action Engine funzionante

## Test regressione

- [ ] Control Room
- [ ] Analytics
- [ ] Medical
- [ ] Branding
- [ ] AI Intent Catalog
- [ ] Repository Toolkit

## Criterio di validazione

Ogni sprint Workspace 2.0 è valido solo se:

- build superata;
- app avviata;
- moduli principali verificati;
- commit e push completati.
