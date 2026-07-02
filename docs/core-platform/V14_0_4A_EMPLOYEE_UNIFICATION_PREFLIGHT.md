# 14.0.4A - Employee Unification Preflight

## Obiettivo

Preparare l'unificazione dei dipendenti senza modificare ancora database, servizi o UI.

## Problema da risolvere

Oggi il progetto ha due anagrafiche dipendenti:

```text
Asset Management
  accyourate-assets.db
    Employees

Master Data
  accyourate-master-data.db
    Employees
```

Questa duplicazione impedisce una relazione sicura tra Asset e Dipendenti.

## Strategia corretta

Prima di scrivere codice di migrazione dobbiamo verificare i database reali creati dall'app sul PC dell'utente.

## File aggiunti

- `scripts/find-accyourate-databases.ps1`
- `scripts/inspect-sqlite-schema.ps1`
- documentazione tecnica di preflight

## Cosa NON cambia

- Nessuna UI modificata.
- Nessun database modificato.
- Nessun servizio modificato.
- Nessuna migrazione eseguita.
- Nessuna tabella eliminata.

## Procedura

### 1. Trovare i database

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\find-accyourate-databases.ps1
```

### 2. Ispezionare la cartella dati

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\inspect-sqlite-schema.ps1
```

## Output atteso

Dobbiamo individuare almeno:

```text
accyourate-assets.db
accyourate-master-data.db
```

## Decisione successiva

Dopo il preflight, la 14.0.4B potrà introdurre una migrazione sicura:

1. backup dei database;
2. lettura dipendenti Master Data;
3. sincronizzazione tabella Employees del database Asset;
4. mantenimento FK `AssetAssignments.EmployeeId`;
5. nessuna eliminazione immediata dei dati legacy.

## Perché non unifichiamo subito

Una modifica diretta ai database senza preflight rischierebbe:

- perdita dati;
- vincoli FK errati;
- duplicati;
- corruzione delle assegnazioni future;
- regressioni su Asset Management o Master Data.

Questa fase è quindi obbligatoria per un refactoring sicuro.
