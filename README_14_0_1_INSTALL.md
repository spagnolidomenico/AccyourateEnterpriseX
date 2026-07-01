# Installazione 14.0.1 - Architecture Baseline

## Come installare

1. Estrai lo ZIP.
2. Copia nel repository:
   - `src`
   - `docs`
   - `tests`
   - `AccyourateEnterpriseX.sln`
   - `README_14_0_1_INSTALL.md`

3. Esegui:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\test-smoke.ps1
```

## Commit consigliato

```powershell
git add .
git commit -m "CORE-001: Add architecture baseline projects"
git push
```
