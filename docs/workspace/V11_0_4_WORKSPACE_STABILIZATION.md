# Sprint 11.0.4 - Workspace Stabilization

## Obiettivo

Consolidare la gestione delle schede dopo la migrazione di Dashboard e Digital Twin.

## Cosa cambia

- Introduzione di un `WorkspaceTabManager` centrale per i moduli già migrati.
- Dashboard e Digital Twin usano lo stesso `WorkspaceHost`.
- Aggiunto `WorkspaceState` foundation.
- Aggiunti metodi di snapshot nello `WorkspaceTabManager`.
- Aggiunta gestione errore di apertura modulo con tab di errore leggibile.

## Cosa NON cambia

- AI Assistant resta invariato.
- Action Engine resta invariato.
- Universal Command Bar resta invariata.
- Non viene ancora introdotta persistenza su disco.

## Criteri di accettazione

- Build riuscita.
- Dashboard funziona ancora come tab.
- Digital Twin funziona ancora come tab.
- Dashboard e Digital Twin possono coesistere nello stesso host tab.
- Nessuna regressione sui moduli principali.

## Prossimo sprint

11.0.5 - AI Assistant come scheda interna.
