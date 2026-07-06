# Test Foundation

## Obiettivo

Definire la base dei futuri test automatici.

## Aree da coprire

### Settings

- Caricamento impostazioni default
- Salvataggio impostazioni
- Generazione numerazioni

### Backup

- Creazione backup
- Presenza manifest
- Presenza checksum
- Verifica integrità

### Document Center

- Registrazione documento
- Ricerca documento
- Lettura documenti recenti

### Search

- Query vuota restituisce zero risultati
- Query con meno di due caratteri restituisce zero risultati
- Query valida non genera eccezioni

### Update

- Lettura manifest locale
- Export manifest
- Lettura release notes

## Strategia consigliata

Nel prossimo sprint tecnico sarà possibile aggiungere un progetto test dedicato, ad esempio:

```text
tests/Accyourate.App.Tests
```

Framework consigliato:

- xUnit
- FluentAssertions
- database SQLite temporanei
