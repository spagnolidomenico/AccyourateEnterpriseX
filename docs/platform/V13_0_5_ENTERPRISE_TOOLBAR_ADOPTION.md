# Sprint 13.0.5 - EnterpriseToolbar Adoption

## Obiettivo

Migrare la toolbar di Asset Management al componente condiviso `EnterpriseToolbar`.

## Modifica effettuata

La toolbar di Asset Management usa ora:

- `AddSecondary` per Aggiorna;
- `AddPrimary` per Nuovo Asset;
- `AddPlaceholder` per Importa Excel;
- `AddPlaceholder` per Esporta Excel.

## Cosa cambia

- Ridotta duplicazione UI locale.
- La toolbar inizia a seguire lo standard del nuovo Enterprise UI Framework.

## Cosa NON cambia

- Nessuna modifica al database.
- Nessuna modifica al CRUD Asset.
- Nessuna modifica al Master Data.
- Nessuna modifica alla Workspace.

## Criteri di accettazione

- Build locale superata.
- GitHub Actions verde.
- Asset Management si apre.
- Pulsante Aggiorna funziona.
- Pulsante + Nuovo funziona.
- Nuovo/Modifica/Elimina Asset funzionano.
- Nessuna regressione sui moduli principali.
