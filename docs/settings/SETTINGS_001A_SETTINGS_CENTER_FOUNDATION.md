# SETTINGS-001A - Settings Center Foundation

## Obiettivo

Introdurre il primo Settings Center persistente della piattaforma.

## Componenti introdotti

- `ApplicationSettings`
- `CompanySettings`
- `NumberingSettings`
- `DocumentSettings`
- `SettingsService`
- `SettingsCenterView`
- `SettingsWorkspaceModule`

## File impostazioni

Le impostazioni vengono salvate in:

```text
%APPDATA%/AccyourateEnterpriseX/settings.json
```

## Sezioni UI

- Azienda
- Numerazioni
- Documenti
- Informazioni

## Cosa NON cambia ancora

- Il PDF Engine non legge ancora il Settings Center.
- Il Branding Center non legge ancora il Settings Center.
- Nessun logo viene ancora applicato automaticamente ai PDF.

## Prossimo sprint

`SETTINGS-001B - Connect Settings to PDF Engine`
