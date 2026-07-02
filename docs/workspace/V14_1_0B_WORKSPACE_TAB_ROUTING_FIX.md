# 14.1.0B - Workspace Tab Routing Fix

## Obiettivo

Uniformare l'apertura dei moduli principali nella Workspace come tab interne.

## Moduli corretti

- AI Intent Catalog
- Analytics
- Medical Device Suite
- Branding Center
- Design System
- Enterprise Architecture

## Cosa cambia

Questi moduli non vengono più aperti come finestre esterne dalla sidebar Workspace.
Vengono instradati tramite `OpenWorkspaceModuleTab`.

## Cosa NON cambia

- Nessuna modifica database.
- Nessuna modifica Asset Assignment Engine.
- Nessuna modifica CRUD.
- Nessuna migrazione dati.

## Test

- Aprire ogni modulo dalla sidebar Workspace.
- Verificare che compaia come tab interna.
- Verificare che non si aprano finestre esterne.
