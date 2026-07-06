# RC1-001 - Quality Foundation

## Obiettivo

Avviare la fase Release Candidate introducendo documentazione, checklist e script di controllo qualità.

## Contenuto

- `VERSION`
- `CHANGELOG.md`
- `RELEASE_NOTES.md`
- `scripts/quality-check.ps1`
- `tests/quality/RC1_Regression_Checklist.md`
- `tests/quality/Test_Foundation.md`

## Quality Gate minimo

Prima di ogni commit RC:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\quality-check.ps1
```

## Controlli richiesti

- Build progetto App.
- Smoke test.
- Verifica presenza `VERSION`.
- Verifica presenza `CHANGELOG.md`.
- Verifica presenza `RELEASE_NOTES.md`.
- Revisione manuale regressione moduli principali.

## Prossimo sprint

`RC1-002 - UI/UX Stabilization`
