# ADR-0011 - Dashboard come primo IWorkspaceModule

## Stato

Accettato

## Contesto

Dopo aver introdotto `IWorkspaceModule` e `WorkspaceModuleRegistry`, serve migrare un primo modulo reale per validare l'architettura.

## Decisione

La Dashboard viene registrata come primo `IWorkspaceModule`.

## Motivazione

La Dashboard è il modulo meno rischioso da migrare e permette di verificare il registry senza modificare funzionalità critiche.

## Conseguenze

- La Dashboard diventa il primo modulo autodescrittivo.
- Il registry viene usato nel flusso reale.
- I moduli successivi potranno seguire lo stesso pattern.
