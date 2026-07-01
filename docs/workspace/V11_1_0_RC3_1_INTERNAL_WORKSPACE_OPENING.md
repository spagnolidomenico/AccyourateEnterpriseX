# 11.1.0 RC3.1 - Internal Workspace Opening Hotfix

## Problema

AI Assistant, Action Engine e Universal Command Bar potevano ancora aprirsi come finestre esterne da alcuni pulsanti della top bar e della sidebar.

## Correzione

- Top button `AI` ora apre `ai-assistant` tramite `Navigate`.
- Top button `Command` ora apre `universal-command-bar` tramite `Navigate`.
- Sidebar `AI Assistant` ora usa `AddMenu`.
- Sidebar `Action Engine` ora usa `AddMenu`.
- Sidebar `Universal Command Bar` ora usa `AddMenu`.
- Aggiunto `UniversalCommandBarView`.
- `UniversalCommandBarWindow` diventa wrapper della view.
- Aggiunto `UniversalCommandBarWorkspaceModule`.

## Risultato atteso

- AI Assistant si apre come tab Workspace.
- Action Engine si apre come tab Workspace.
- Universal Command Bar si apre come tab Workspace.
- Non devono più aprirsi finestre esterne dai pulsanti principali.
