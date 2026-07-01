# Installazione 13.1.0a - Master Data Employees CRUD

## Come installare

1. Estrai lo ZIP.
2. Copia nel repository:
   - `src`
   - `docs`
   - `tests`
   - `README_13_1_0a_INSTALL.md`

3. Esegui:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\test-smoke.ps1
```

## Commit consigliato

```powershell
git add .
git commit -m "MASTER-002: Add Employees CRUD foundation"
git push
```
