# Database Versioning

Da Developer 3.1 il database contiene:

- `database_versions`
- `app_settings`

La tabella `database_versions` permette di sapere quale baseline è stata applicata.

La tabella `app_settings` contiene configurazioni globali come:
- nome azienda;
- colore tema;
- backup;
- canale release.
