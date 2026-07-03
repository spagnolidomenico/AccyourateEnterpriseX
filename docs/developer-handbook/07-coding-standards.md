# Coding Standards

## C#

- Classi piccole e focalizzate.
- Metodi leggibili.
- Nomi espliciti.
- Evitare logica business nelle View.
- Query SQL parametrizzate.
- Errori tecnici nei log.
- Errori operativi con messaggio comprensibile.

## Async

- Evitare chiamate async non attese.
- Correggere warning `CS4014`.
- Usare `async Task` dove possibile.
- Usare `async void` solo per event handler UI.
