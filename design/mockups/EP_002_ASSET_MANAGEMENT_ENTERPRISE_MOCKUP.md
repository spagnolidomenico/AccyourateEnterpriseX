# Mockup - Asset Management Enterprise

## Obiettivo
Rendere Asset Management il modulo campione del nuovo design.

```text
┌──────────────────────────────────────────────────────────────────────────────┐
│ IT Asset Management                                                          │
│ Gestione del patrimonio informatico aziendale                                │
│                                                                              │
│ Cerca asset, seriale, assegnatario...                      [+ Nuovo Asset]   │
├──────────────────────────────────────────────────────────────────────────────┤
│ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐              │
│ │ Totali      │ │ Disponibili │ │ Assegnati   │ │ Manutenz.   │              │
│ │   312       │ │   145       │ │   158       │ │     9       │              │
│ └─────────────┘ └─────────────┘ └─────────────┘ └─────────────┘              │
├──────────────────────────────────────────────────────────────────────────────┤
│ Filtri: Categoria ▼    Stato ▼    Utente ▼    Marca ▼       [Aggiorna]      │
├───────────────────────────────────────────────┬──────────────────────────────┤
│ Tabella asset                                  │ Scheda tecnica asset         │
│ Codice | Categoria | Marca | Modello | Stato   │ Informazioni principali      │
│ AST-01 | Notebook  | Dell  | 7420    | Disp.   │ Categoria: Notebook          │
│ AST-02 | Desktop   | HP    | Elite   | Ass.    │ Marca: Dell                  │
└───────────────────────────────────────────────┴──────────────────────────────┘
```

## Correzione UI-002
Le disponibilità vanno rappresentate con KPI card identiche: stessa larghezza, altezza, font, icona e allineamento numerico.
