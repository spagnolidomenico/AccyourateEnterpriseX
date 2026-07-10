# EP-005B-R1 - KPI Layout Fix

## Problema

I sottotitoli delle KPI card della Enterprise Dashboard, ad esempio `0 attivi`, `0 assegnati` e `0 PDF generati`, uscivano dal bordo inferiore.

## Correzione

- `AxKpiCard` usa ora una `Grid` con quattro righe dedicate:
  - icona;
  - valore;
  - titolo;
  - sottotitolo.
- Altezza minima aumentata a 184 px.
- Il sottotitolo rimane ancorato all'interno della card.
- La Dashboard usa il componente condiviso `AxKpiCard` invece di una KPI locale duplicata.
- Il `WrapPanel` della Dashboard riserva 196 px per ogni elemento.

## Verifica manuale

Aprire Enterprise Dashboard e controllare che:

- nessun testo esca dalle card;
- i sottotitoli siano interamente visibili;
- le sei card abbiano altezza uniforme;
- il layout resti leggibile con scaling Windows 125% e 150%.
