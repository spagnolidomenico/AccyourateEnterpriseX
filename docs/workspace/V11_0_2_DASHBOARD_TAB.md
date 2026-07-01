# Sprint 11.0.2 - Dashboard Tab

## Obiettivo

Migrare solo la Dashboard nella nuova infrastruttura a schede introdotta nello Sprint 11.0.1.

## Cosa cambia

- La voce `Dashboard` nella Enterprise Workspace apre un `WorkspaceHost`.
- La Dashboard viene caricata come tab interna.
- La scheda Dashboard è pinnata e non chiudibile.
- Se la Dashboard è già aperta, viene riattivata e non duplicata.

## Cosa NON cambia

- Digital Twin resta invariato.
- AI Assistant resta invariato.
- Action Engine resta invariato.
- Universal Command Bar resta invariata.
- Control Room resta invariata.

## Criteri di accettazione

- Build riuscita.
- App avviata.
- Dashboard aperta come scheda.
- Click ripetuti su Dashboard non duplicano la scheda.
- Nessuna regressione sui moduli principali.

## Prossimo sprint

11.0.3 - Digital Twin come scheda interna.
