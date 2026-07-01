# ENG-001 - GitHub Actions Build Automation

## Obiettivo

Aggiungere una pipeline di build automatica su GitHub.

## Cosa fa

La workflow viene eseguita automaticamente su:

- push su `main`;
- push su `develop`;
- push su branch `feature/**`;
- pull request verso `main` o `develop`.

## Passaggi eseguiti

1. Checkout repository.
2. Installazione .NET 8 SDK.
3. Visualizzazione info .NET.
4. Restore pacchetti NuGet.
5. Build della soluzione in configurazione Release.

## File aggiunto

```text
.github/workflows/build.yml
```

## Come verificare

1. Copia la patch nel repository.
2. Fai commit.
3. Push su GitHub.
4. Vai nella tab `Actions`.
5. Verifica che la build parta automaticamente.

## Risultato atteso

```text
Build .NET Solution ✅
```

## Note

Questa pipeline non pubblica ancora installer o release.
Serve come primo Quality Gate automatico.
