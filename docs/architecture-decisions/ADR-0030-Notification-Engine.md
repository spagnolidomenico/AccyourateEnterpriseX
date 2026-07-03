# ADR-0030 - Notification Engine

## Stato

Accettato

## Decisione

Introdurre un `NotificationService` centralizzato con persistenza SQLite dedicata.

## Motivazione

- Evitare notifiche sparse nei singoli moduli.
- Preparare Notification Center.
- Alimentare Home Widget e Activity Timeline.
- Consentire integrazioni future con AI, Workflow e Audit.
