# ADR-0002 - Action Engine basato su Capability Registry

## Stato

Accettato

## Contesto

L'AI Assistant deve poter eseguire azioni operative senza chiamare direttamente i moduli applicativi.

## Decisione

Introduciamo un Action Engine con Capability Registry.

Ogni modulo dichiara le capability disponibili. L'Action Engine esegue solo capability registrate e autorizzate.

## Conseguenze

- L'AI non può eseguire codice arbitrario.
- Le azioni sono tracciabili e controllabili.
- Nuovi moduli possono esporre capability senza modificare l'AI Assistant.
- Le operazioni di modifica dati richiederanno conferma e permessi granulari nelle prossime RC.
