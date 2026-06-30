# Installazione Repository Toolkit v1.0

## Come installare

1. Estrai questo ZIP.
2. Copia le cartelle:
   - `scripts`
   - `docs`
3. Incollale nella cartella principale del repository `AccyourateEnterpriseX`.
4. Se Windows chiede di unire le cartelle, conferma.
5. Apri PowerShell nella cartella del repository.
6. Esegui:

```powershell
.\scripts\test-smoke.ps1
```

## Se PowerShell blocca gli script

Esegui una sola volta:

```powershell
Set-ExecutionPolicy -Scope CurrentUser RemoteSigned
```

Poi premi `S`.

## Commit consigliato

```text
DEV-001: Add repository PowerShell toolkit
```
