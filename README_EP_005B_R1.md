# EP-005B-R1 KPI Layout Fix

Obiettivo:
- Aumentare l'altezza minima delle KPI Card.
- Usare una Grid a 4 righe (Icona, Valore, Titolo, Sottotitolo).
- Evitare che il sottotitolo esca dal bordo.
- Aggiungere margine inferiore e TextWrapping.

Interventi consigliati:
- MinHeight: 170-180 px
- Padding: 20 px
- RowDefinitions: Auto,Auto,Auto,*
- VerticalAlignment=Stretch
- TextWrapping=Wrap
- TextTrimming=None
