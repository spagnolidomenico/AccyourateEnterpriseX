# Installazione 13.0.5.1 - EnterpriseToolbar Click Hotfix

## Come installare

Copia nel repository:
- `src`
- `docs`
- `README_13_0_5_1_INSTALL.md`

Poi esegui:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\test-smoke.ps1
```

Commit consigliato:

```powershell
git add .
git commit -m "FIX-004: Prevent EnterpriseToolbar click propagation"
git push
```
