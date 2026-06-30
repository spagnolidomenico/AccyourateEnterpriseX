# Accyourate Enterprise X - RC 6.1.4.2 Enterprise Navigation Clean Hotfix

## Correzione

Questa versione riparte dalla RC 6.1.3 validata e applica solo modifiche sicure:

- icone nei titoli del menu;
- sezioni più intuitive;
- guida Enterprise Navigation;
- contrasto migliorato.

## Nota tecnica

Non viene riscritto il metodo `AddMenuButton`, evitando gli errori introdotti nella 6.1.4.1:

- `CurrentUser.HasPermission`
- `CS0149`
- duplicazioni proprietà dei pulsanti.
