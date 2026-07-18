# Installazione EP-005C

Copia i file della patch nella radice del repository e sovrascrivi quelli esistenti.

Esegui:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\test-asset-professional-ui.ps1
```

Poi avvia l'app:

```powershell
.\scripts\run.ps1
```

Commit, solo dopo verifica visiva:

```powershell
git add .
git commit -m "EP-005C: Refine Asset Management professional UI"
git push
```
