# RC1-002C-R1 - Enterprise Tab Bar

## Problema

Quando molte tab sono aperte, la scrollbar orizzontale può sovrapporsi al testo delle tab e renderne difficile la lettura.

## Correzioni

- Area dedicata alla scrollbar con padding inferiore.
- Altezza minima della tab bar.
- Tab con larghezza minima.
- Titolo tab con ellissi.
- Tooltip con nome completo della tab.
- Pulsanti laterali `◀` e `▶` per scorrere rapidamente.
- Layout azioni separato dalla riga delle tab.

## File modificati

- `WorkspaceHost.cs`
- `WorkspaceTabHost.cs`

## Test

Aprire almeno 12-15 schede e verificare:

- la scrollbar non copre il nome delle tab;
- i titoli lunghi vengono troncati con ellissi;
- il tooltip mostra il titolo completo;
- i pulsanti `◀` e `▶` scorrono la tab bar.
