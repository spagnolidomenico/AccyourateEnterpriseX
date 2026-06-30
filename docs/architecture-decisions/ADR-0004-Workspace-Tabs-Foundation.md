# ADR-0004 - Workspace Tabs Foundation

## Stato

Accettato

## Contesto

Accyourate deve evolvere da interfaccia con finestre multiple a Workspace unica con schede.

## Decisione

Introdurre prima i componenti base delle schede senza cambiare il comportamento utente.

## Motivazione

Questo approccio riduce il rischio di regressioni e permette di testare l'infrastruttura prima di migrare i moduli.

## Conseguenze

- La base tecnica è pronta.
- I moduli saranno migrati uno alla volta.
- La Workspace resterà stabile durante la transizione.
