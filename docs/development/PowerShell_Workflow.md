# Workflow PowerShell - Accyourate Enterprise X

## Build completa

Dalla cartella principale del repository:

```powershell
.\scripts\build.ps1
```

## Avvio applicazione

```powershell
.\scripts\run.ps1
```

## Smoke test

```powershell
.\scripts\test-smoke.ps1
```

## Release check

```powershell
.\scripts\release-check.ps1 -Version "10.1 RC1"
```

## Note

Se PowerShell blocca l'esecuzione degli script, esegui una sola volta:

```powershell
Set-ExecutionPolicy -Scope CurrentUser RemoteSigned
```

Poi conferma con `S`.
