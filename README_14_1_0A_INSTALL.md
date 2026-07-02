# Installazione 14.1.0A - Asset Assignment Engine Foundation

Copia nel repository:

- `src`
- `scripts`
- `docs`
- `tests`
- `README_14_1_0A_INSTALL.md`

Poi esegui:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\test-smoke.ps1
```

Facoltativo:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\test-assignment-engine.ps1
```

Commit consigliato:

```powershell
git add .
git commit -m "ASSET-001: Add asset assignment engine foundation"
git push
```
