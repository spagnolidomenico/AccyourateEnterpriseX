# Test Plan - Developer Edition 1.1

## Obiettivo

Validare la versione 1.1 prima di promuoverla a Release Candidate.

## Casi di test

### TC-001 Build
Eseguire clean, restore e build.

Risultato atteso:
- nessun errore di compilazione.

### TC-002 Login Admin
Accedere con admin / admin123.

Risultato atteso:
- login riuscito;
- menu completo.

### TC-003 Creazione utente
Creare utente `test` con password `Test1234`.

Risultato atteso:
- utente creato;
- visibile in lista.

### TC-004 Login utente creato
Accedere con `test / Test1234`.

Risultato atteso:
- login riuscito.

### TC-005 Menu filtrato
Accedere con utente Operatore.

Risultato atteso:
- menu più limitato rispetto ad Admin.

### TC-006 Cambio password
Cambiare password da `Test1234` a `Password2026`.

Risultato atteso:
- vecchia password non valida;
- nuova password valida.

### TC-007 Diagnostica
Aprire Diagnostica.

Risultato atteso:
- percorso DB;
- utenti;
- audit log.
