# EP-005B-R1 - Workspace Home Widget Fix

## Problema
I testi secondari dei KPI della Workspace Home, come `0 attivi`, `0 assegnati` e `0 PDF generati`, uscivano visivamente dal riquadro o risultavano tagliati.

## Correzioni
- `AxKpiCard` ricostruito con Grid a quattro righe.
- Altezza fissa e coerente: 186 px.
- Numero aumentato a 40 px.
- Titolo e sottotitolo centrati e con wrapping.
- Padding inferiore di sicurezza.
- `ClipToBounds` sulla card.
- Workspace Home aggiornata a `ItemHeight = 202`.
- Enterprise Dashboard aggiornata allo stesso componente condiviso.

## Test manuale
Aprire Workspace Home con scaling Windows 100%, 125% e 150% e verificare che tutti i testi restino dentro le card.
