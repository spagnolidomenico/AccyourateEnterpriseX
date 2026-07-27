# M6.1.1 — Clickable Asset Cards

Correzione della vista Card di Asset Management.

## Modifiche

- Rimosso il pulsante blu `Apri dettaglio` dalle card.
- L'intera card è ora cliccabile.
- Clic singolo: seleziona la card.
- Doppio clic: apre il pannello di dettaglio.
- La card selezionata viene evidenziata con il bordo blu.
- Ridotta l'altezza delle card per recuperare spazio verticale.
- Aggiunta una breve indicazione d'uso nella parte inferiore della card.

## Avvio

Chiudere l'applicazione, quindi eseguire:

```powershell
dotnet clean
dotnet build
dotnet run --project src/Accyourate.App/Accyourate.App.csproj
```
