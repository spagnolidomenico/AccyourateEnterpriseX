# Contributing - Accyourate Enterprise X

## Branch strategy

- `main`: solo release stabili.
- `develop`: integrazione della prossima versione.
- `feature/...`: sviluppo di singole funzionalità.
- `fix/...`: correzioni mirate.
- `release/...`: stabilizzazione pre-release.

## Regola principale

Non sviluppare direttamente su `main`.

## Naming branch

Esempi:

```text
feature/13.0.1-enterprise-ui-framework
feature/master-data-crud
fix/asset-dialog-validation
```

## Convenzione commit

Usare prefissi leggibili:

```text
UI-001: Add Enterprise UI Framework foundation
FIX-001: Resolve Asset dialog crash
ASSET-001: Add Asset Management module
MASTER-001: Add Master Data service
CORE-001: Register workspace module
DATA-001: Add asset database schema
DOC-001: Add architecture documentation
TEST-001: Add service tests
```

## Prima di ogni commit

- Build locale superata.
- App avviata almeno una volta se la modifica tocca la UI.
- Nessuna modifica accidentale a file non collegati.
- Documentazione aggiornata se necessario.

## Prima del merge in develop

- GitHub Actions verde.
- Checklist della feature completata.
- Nessuna regressione sui moduli principali.
- CHANGELOG aggiornato.

## Prima del merge in main

- Release test completa.
- Tag Git pronto.
- Documentazione release aggiornata.
