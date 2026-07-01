# ADR-0012 - Workspace Integration tramite Module Registry

## Stato

Accettato

## Contesto

Dopo l'introduzione di `IWorkspaceModule`, è necessario migrare i moduli principali al registry.

## Decisione

Dashboard, Digital Twin, AI Assistant e Action Engine vengono registrati come moduli della Workspace.

## Motivazione

- Ridurre logiche speciali nella Workspace.
- Uniformare apertura dei moduli.
- Preparare Explorer, preferiti, persistenza e plugin futuri.
- Ridurre l'uso di finestre separate per moduli principali.

## Nota su Universal Command Bar

La Universal Command Bar è un componente operativo diverso dai moduli standard.
Verrà integrata in modo dedicato come overlay/command palette interna in una RC successiva.
