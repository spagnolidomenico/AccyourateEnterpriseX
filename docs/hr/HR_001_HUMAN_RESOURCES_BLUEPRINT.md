# HR-001 - Human Resources Blueprint

## Obiettivo

Creare il modulo Human Resources come centro operativo della piattaforma.

Il modulo HR diventa la fonte principale per:

- persone;
- ruoli;
- reparti;
- sedi;
- contratti;
- documenti;
- scadenze;
- collegamento con gli asset aziendali.

---

# Funzionalità HR v1.0

## Dipendenti

Campi iniziali:

- Id
- Nome
- Cognome
- Nome completo
- Email
- Telefono
- Ruolo
- Reparto
- Sede
- Responsabile
- Stato
- Data assunzione
- Data cessazione
- Note

## Organizzazione

Entità:

- Company
- Site
- Department
- Role
- Manager relationship

## Contratti

Campi iniziali:

- Tipo contratto
- Data inizio
- Data fine
- Livello
- Mansione
- Note

## Documenti dipendente

Tipologie:

- contratto;
- documento identità;
- codice fiscale;
- attestati;
- formazione;
- verbali consegna beni;
- altri allegati.

## Scadenze

Esempi:

- fine contratto;
- rinnovo documenti;
- scadenza formazione;
- revisione assegnazioni.

---

# Integrazioni

## Asset Management

Ogni dipendente potrà avere:

- notebook;
- desktop;
- smartphone;
- monitor;
- badge;
- DPI;
- licenze;
- altri beni.

## Notification Service

Eventi che generano notifiche:

- nuovo dipendente creato;
- contratto in scadenza;
- documento mancante;
- formazione in scadenza;
- asset assegnato.

## Audit Service

Azioni tracciate:

- creazione dipendente;
- modifica dipendente;
- eliminazione/disattivazione;
- assegnazione asset;
- modifica contratto;
- caricamento documento.

---

# Criteri di completamento HR-001

HR-001 è completo quando esistono:

- blueprint HR;
- modello dati definito;
- struttura cartelle prevista;
- integrazioni definite;
- checklist di test;
- roadmap HR v1.0.
