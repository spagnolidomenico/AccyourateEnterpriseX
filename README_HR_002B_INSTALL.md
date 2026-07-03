# Installazione HR-002B - Database & Repository

Copia nel repository:

- `src`
- `scripts`
- `docs`
- `tests`
- `README_HR_002B_INSTALL.md`

Poi esegui:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\test-smoke.ps1
```

Facoltativo:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\test-hr-repository.ps1
```

Commit consigliato:

```powershell
git add .
git commit -m "HR-002B: Add HR database and repositories"
git push
```
