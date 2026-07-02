# Installazione 14.0.4A - Employee Unification Preflight

## Come installare

Copia nel repository:

- `scripts`
- `docs`
- `tests`
- `PROJECT_STATUS.md`
- `README_14_0_4A_INSTALL.md`

## Comandi da eseguire

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\find-accyourate-databases.ps1
```

Poi:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\inspect-sqlite-schema.ps1
```

## Commit consigliato

```powershell
git add .
git commit -m "CORE-004: Add employee unification preflight"
git push
```
