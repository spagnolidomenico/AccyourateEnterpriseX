# Regole di refactoring UI

## Regola 1

Non aggiungere più finestre complesse costruite interamente in C#.

## Regola 2

Ogni nuova schermata deve usare componenti comuni:

- tokens;
- card;
- buttons;
- layout;
- typography;
- shell modules.

## Regola 3

Le funzionalità già validate non devono essere riscritte in blocco.

## Regola 4

Migrare un modulo alla volta e testare ogni step.

## Regola 5

La shell enterprise diventerà gradualmente la finestra principale.
