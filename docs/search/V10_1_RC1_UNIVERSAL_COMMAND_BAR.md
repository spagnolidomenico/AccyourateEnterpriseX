# Versione 10.1 RC1 - Universal Command Bar & Enterprise Search Service

## Obiettivo

Creare un punto unico per cercare, aprire moduli ed eseguire azioni tramite Action Engine.

## Componenti

- SearchResult
- ISearchProvider
- EnterpriseSearchService
- DigitalTwinSearchProvider
- UniversalCommandBarWindow

## Integrazione

- Workspace top bar
- Sidebar Workspace
- Command Palette
- Main Window
- Action Engine

## Test

Comandi consigliati:

```text
TOP
TOP001
offline
batteria
ECG
telemetria
```

Selezionando un risultato viene eseguita la capability corrispondente nell'Action Engine.
