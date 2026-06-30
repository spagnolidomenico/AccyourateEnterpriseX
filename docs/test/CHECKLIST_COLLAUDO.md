# Checklist di collaudo Accyourate Enterprise X

## Build

- [ ] `dotnet clean AccyourateEnterpriseX.sln`
- [ ] `dotnet restore AccyourateEnterpriseX.sln`
- [ ] `dotnet build AccyourateEnterpriseX.sln`
- [ ] Build terminata senza errori

## Avvio

- [ ] `dotnet run --project src\Accyourate.App\Accyourate.App.csproj`
- [ ] Finestra login aperta
- [ ] Login admin riuscito
- [ ] Centro Operativo aperto

## Database

- [ ] Database presente in `C:\ProgramData\Accyourate Enterprise X\data\accyourate_x.db`
- [ ] Diagnostica aperta
- [ ] Numero utenti visualizzato
- [ ] Audit log visualizzato

## Utenti

- [ ] Gestione Utenti aperta
- [ ] Nuovo utente creato
- [ ] Nuovo utente visibile nella lista
- [ ] Cambio ruolo funzionante
- [ ] Disattivazione funzionante
- [ ] Admin principale non disattivabile

## Ruoli e permessi

- [ ] Utente Admin vede tutte le voci
- [ ] Utente Operatore vede menu ridotto
- [ ] Utente Lettura vede menu ridotto
- [ ] Cambio Password funzionante

## Regressione

- [ ] Login continua a funzionare
- [ ] Database non viene cancellato
- [ ] Utenti già creati restano presenti
- [ ] Diagnostica continua a funzionare
