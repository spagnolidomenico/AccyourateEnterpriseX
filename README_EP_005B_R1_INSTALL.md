# Installazione EP-005B-R1 - KPI Layout Fix

Copia la patch nella root del repository, sovrascrivendo i file esistenti.

Esegui:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\test-kpi-layout-fix.ps1
```

Commit:

```powershell
git add .
git commit -m "EP-005B-R1: Fix KPI card layout overflow"
git push
```
