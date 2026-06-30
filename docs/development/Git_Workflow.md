# Git Workflow - Accyourate Enterprise X

## Branch principali

- `main`: solo versioni stabili.
- `develop`: sviluppo quotidiano.

## Flusso consigliato

1. Lavora su `develop`.
2. Copia o modifica i file.
3. Esegui:

```powershell
.\scripts\test-smoke.ps1
```

4. Se tutto è OK, commit:

```text
AX-001: descrizione modifica
```

5. Push origin.

## Quando creare una Release GitHub

Solo per milestone importanti:

- v10.0
- v11.0
- v12.0 Beta
