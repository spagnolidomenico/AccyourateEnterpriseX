# Installazione 15.0.1C.1 - Notification Engine

Copia nel repository:

- `src`
- `scripts`
- `docs`
- `tests`
- `README_15_0_1C1_INSTALL.md`

Poi esegui:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\test-smoke.ps1
```

Facoltativo:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\test-notification-engine.ps1
```

Commit consigliato:

```powershell
git add .
git commit -m "PLATFORM-001: Add notification engine"
git push
```
