# ADR-0008 - AI Assistant come View riutilizzabile

## Stato

Accettato

## Contesto

AI Assistant era implementato come finestra separata. Per integrarlo nella Workspace a schede serve un contenuto riutilizzabile.

## Decisione

Estrarre l'interfaccia dell'AI Assistant in `EnterpriseAiAssistantView`.

`EnterpriseAiAssistantWindow` diventa un wrapper che ospita la stessa view.

## Motivazione

- Riduce duplicazione.
- Permette di aprire AI Assistant come tab interna.
- Mantiene compatibilità con eventuale apertura come finestra.
- Prepara l'integrazione futura con Action Engine e Universal Command Bar.

## Conseguenze

AI Assistant diventa un modulo UI riutilizzabile nella Workspace.
