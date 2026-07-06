# Installazione RC1-001 - Quality Foundation

Copia nel repository:

- `VERSION`
- `CHANGELOG.md`
- `RELEASE_NOTES.md`
- `docs`
- `tests`
- `scripts`
- `README_RC1_001_INSTALL.md`

Poi esegui:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\quality-check.ps1
```

Commit:

```powershell
git add .
git commit -m "RC1-001: Add quality foundation"
git push
```
