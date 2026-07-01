# Accyourate Enterprise X - Product Constitution

## Scopo

La Product Constitution definisce le regole fondamentali che guidano ogni decisione tecnica e funzionale.

## Principi

### 1. Modularità prima di tutto

Ogni funzionalità deve essere progettata come parte di un modulo.

### 2. Workspace-first

Ogni modulo principale deve aprirsi nella Workspace come tab.

### 3. UI coerente

Le schermate devono usare i componenti del UI Framework condiviso.

### 4. Dati separati dalla UI

La UI non deve contenere logica business complessa.

### 5. Servizi testabili

Ogni servizio deve poter essere testato senza dipendere dall'interfaccia grafica.

### 6. Documentazione viva

Ogni decisione importante deve aggiornare documentazione, ADR o roadmap.

### 7. Git disciplinato

`main` è stabile, `develop` integra, `feature/*` sviluppa.

### 8. Release tracciabili

Ogni release stabile deve avere tag Git e changelog.

### 9. Sicurezza come requisito

Password, token e segreti non devono essere salvati in chiaro.

### 10. Evoluzione futura

Ogni scelta deve considerare compatibilità futura con Web, Mobile, Cloud e AI.
