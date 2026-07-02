# BRAND-003 - Login Company Logo

## Obiettivo

Aggiungere un'area logo aziendale nella schermata login brandizzata.

## Funzionalità

- Logo aziendale caricato da `BrandingPreferenceRecord.LogoPath`.
- Ridimensionamento automatico del logo.
- Fallback elegante con iniziali dell'azienda se il logo non è configurato.
- Nome azienda mostrato sotto il logo.
- Nome prodotto mostrato come sottotitolo.
- Footer `Powered by Accyourate`.

## Percorsi fallback

Se `LogoPath` non è configurato, la login cerca:

```text
Assets/Branding/company_logo.png
Assets/Branding/logo.png
```

## Cosa NON cambia

- Nessuna modifica autenticazione.
- Nessuna modifica utenti/password.
- Nessuna modifica database.
- Nessuna modifica Workspace.
