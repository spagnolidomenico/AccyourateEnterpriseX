# Accyourate Enterprise X — Component Library

Versione catalogo: **M6.1**

## Regola principale

Ogni nuovo modulo deve usare i componenti `Ax*` esistenti prima di creare controlli locali. Lo scopo è mantenere identici stile, dimensioni, spaziature e comportamento in Asset, Persone, Medical, Infrastruttura e negli altri moduli.

## Componenti canonici

- `AxButton`: azioni primarie, secondarie, di successo, avviso e pericolo.
- `AxCommandButton`: pulsante compatto per toolbar, selettori di vista e comandi rapidi.
- `AxToolbar`: contenitore standard per gruppi di comandi.
- `AxCard`: superficie standard per contenuti e informazioni.
- `AxKpiCard`: indicatori numerici interattivi.
- `AxSearchBox`: ricerca coerente nei moduli.
- `AxStatusBadge`: rappresentazione semantica degli stati.
- `AxEnterpriseTable`: tabella enterprise con selezione, ordinamento, righe alternate e attivazione tramite doppio clic.
- `AxInspectorPanel`: pannello laterale di dettaglio.
- `AxTimeline`: cronologia delle attività.
- `AxEmptyState`: stato vuoto con messaggio e azione guidata.

## Convenzioni

1. Le toolbar usano `AxCommandButton`.
2. Gli stati usano `AxStatusBadge.FromStatus(...)`.
3. Le liste operative usano `AxEnterpriseTable<T>`.
4. I colori devono provenire da `UiTokens` o dai token Foundation.
5. Non inserire colori esadecimali direttamente nei moduli.
6. I componenti locali sono ammessi solo quando non esiste ancora un equivalente nel Design System; in quel caso il componente deve essere promosso nella libreria appena stabilizzato.

## Prima adozione

Con M6.1, Asset Management usa il pulsante canonico `AxCommandButton` e i badge di stato `AxStatusBadge`. I prossimi moduli dovranno seguire lo stesso standard.
