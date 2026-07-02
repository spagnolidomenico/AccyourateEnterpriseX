# Installazione 14.1.0B - Workspace Tab Routing Fix

Copia nel repository:

- `src`
- `docs`
- `tests`
- `README_14_1_0B_INSTALL.md`

Poi esegui:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\test-smoke.ps1
```

Commit consigliato:

```powershell
git add .
git commit -m "WORKSPACE-001: Route enterprise modules to workspace tabs"
git push
```
