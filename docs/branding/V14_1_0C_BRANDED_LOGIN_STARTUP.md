# 14.1.0C - Branded Login Startup Integration

## Obiettivo

Rendere effettiva la schermata di login personalizzata all'avvio dell'applicazione.

## Problema

La finestra `BrandedSplashLoginWindow` era usata come anteprima dal Branding Center, ma l'applicazione continuava ad avviare `LoginWindow`.

## Correzione

`App.cs` ora avvia `BrandedSplashLoginWindow` con `AuthenticationService`.

`BrandedSplashLoginWindow` è stata resa funzionale:

- campi username/password reali;
- validazione tramite `AuthenticationService`;
- evento `LoginSucceeded`;
- apertura `MainWindow` dopo login corretto;
- mantenimento della modalità anteprima dal Branding Center.

## Cosa NON cambia

- Nessuna modifica database.
- Nessuna modifica utenti/password.
- Nessuna modifica permessi.
- Nessuna modifica Workspace.

## Test

- Avvio app.
- Visualizzazione login brandizzato.
- Login con `admin / admin123`.
- Apertura corretta della MainWindow.
- Anteprima Branding Center ancora funzionante.
