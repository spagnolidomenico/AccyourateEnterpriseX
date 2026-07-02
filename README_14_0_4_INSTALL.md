# Installazione 14.0.4 - Repository Adoption Foundation

Copia nel repository:
- `src`
- `docs`
- `tests`
- `README_14_0_4_INSTALL.md`

Poi esegui:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\test-smoke.ps1
```

Commit consigliato:

```powershell
git add .
git commit -m "CORE-004: Add repository adoption foundation"
git push
```
