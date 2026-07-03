# Installazione ASSET-004A - Delivery Report Foundation

Copia nel repository:

- `src`
- `scripts`
- `docs`
- `tests`
- `README_ASSET_004A_INSTALL.md`

Poi esegui:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\test-smoke.ps1
```

Facoltativo:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\test-delivery-report.ps1
```

Commit consigliato:

```powershell
git add .
git commit -m "ASSET-004A: Add delivery report foundation"
git push
```
