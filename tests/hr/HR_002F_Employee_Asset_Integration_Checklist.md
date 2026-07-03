# Test Checklist - HR-002F Employee Asset Integration

## Build

- [ ] Smoke test superato
- [ ] GitHub Actions verde

## Human Resources

- [ ] Modulo HR si apre
- [ ] Profilo dipendente si apre
- [ ] Sezione Asset assegnati visibile
- [ ] Se dipendente non collegato ad Anagrafica, compare messaggio chiaro
- [ ] Se dipendente collegato ad Anagrafica, compare ID collegamento

## Asset Integration

- [ ] Assegnare un asset in Asset Management a un dipendente Anagrafica
- [ ] Aprire il dipendente HR con stessa email/nome
- [ ] Verificare asset visualizzato nel profilo HR

## Regressione

- [ ] Nuovo dipendente HR funzionante
- [ ] Modifica dipendente HR funzionante
- [ ] Asset Management funzionante
- [ ] Notification Center funzionante
