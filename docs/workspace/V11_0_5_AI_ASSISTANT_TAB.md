# Sprint 11.0.5 - AI Assistant Tab

## Obiettivo

Migrare AI Assistant come scheda interna della Workspace.

## Cosa cambia

- Aggiunto `EnterpriseAiAssistantView`, contenuto riutilizzabile dentro tab o finestra.
- `EnterpriseAiAssistantWindow` ora è solo un wrapper della view.
- La voce AI Assistant nella Workspace apre una scheda interna.
- Se AI Assistant è già aperto, viene riattivato e non duplicato.

## Cosa NON cambia

- Action Engine resta invariato.
- Universal Command Bar resta invariata.
- Digital Twin resta invariato.
- Dashboard resta invariata.

## Criteri di accettazione

- Build riuscita.
- App avviata.
- AI Assistant aperto come scheda.
- Quick prompt funzionanti.
- Invio domanda funzionante.
- Click ripetuti su AI Assistant non duplicano la scheda.
- Nessuna regressione sui moduli principali.

## Prossimo sprint

11.0.6 - Action Engine come scheda interna.
