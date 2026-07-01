# Installazione 13.0.5 - EnterpriseToolbar Adoption

## Come installare

1. Estrai lo ZIP.
2. Copia nel repository:
   - `src`
   - `docs`
   - `tests`
   - `README_13_0_5_INSTALL.md`
3. Esegui:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\test-smoke.ps1
```

## Commit consigliato

```powershell
git add .
git commit -m "UI-003: Adopt EnterpriseToolbar in Asset Management"
git push
```
