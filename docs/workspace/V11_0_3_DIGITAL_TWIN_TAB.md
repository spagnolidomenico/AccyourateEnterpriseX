# Sprint 11.0.3 - Digital Twin Tab

## Obiettivo

Migrare solo il modulo Digital Twin nella nuova infrastruttura a schede della Workspace.

## Cosa cambia

- La voce `Digital Twin` nella Enterprise Workspace apre un `WorkspaceHost`.
- Il modulo Digital Twin viene caricato come tab interna.
- La scheda Digital Twin è chiudibile.
- Se il Digital Twin è già aperto, viene riattivato e non duplicato.

## Cosa NON cambia

- Dashboard resta come tab già validata.
- AI Assistant resta invariato.
- Action Engine resta invariato.
- Universal Command Bar resta invariata.
- Control Room resta invariata.

## Criteri di accettazione

- Build riuscita.
- App avviata.
- Digital Twin aperto come scheda.
- Click ripetuti su Digital Twin non duplicano la scheda.
- La scheda Digital Twin può essere chiusa.
- Nessuna regressione sui moduli principali.

## Prossimo sprint

11.0.4 - AI Assistant come scheda interna.
