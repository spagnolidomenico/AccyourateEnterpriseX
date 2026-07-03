# 15.0.1C.1 - Notification Engine

## Obiettivo

Introdurre un motore notifiche centralizzato, persistente e riutilizzabile da tutti i moduli.

## Componenti introdotti

- `NotificationRecord`
- `NotificationCategory`
- `NotificationPriority`
- `NotificationService`
- `PlatformEvent`
- `PlatformEventBus`

## Database

Il motore usa un database piattaforma dedicato:

```text
%APPDATA%/AccyourateEnterpriseX/accyourate-platform.db
```

## Funzionalità

- Pubblicazione notifiche.
- Lettura ultime notifiche.
- Conteggio notifiche non lette.
- Segna come letta.
- Segna tutte come lette.
- Eliminazione notifica.

## Cosa NON cambia

- Nessuna UI modificata.
- Nessun badge ancora visibile.
- Nessun modulo integrato ancora.
