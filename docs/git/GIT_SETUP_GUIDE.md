# Guida Git - Accyourate Enterprise X

## Primo setup locale

Apri PowerShell nella cartella del progetto ed esegui:

```powershell
git init
git add .
git commit -m "Initial stable baseline v4.0.0"
git branch -M main
git tag v4.0.0-stable
```

## Collegamento a GitHub privato

Dopo aver creato un repository privato su GitHub:

```powershell
git remote add origin https://github.com/TUO-ACCOUNT/AccyourateEnterpriseX.git
git push -u origin main
git push origin v4.0.0-stable
```

## Avvio sviluppo RC 4.1

```powershell
git checkout -b develop
git checkout -b feature/rc-4-1-production-quality
```
