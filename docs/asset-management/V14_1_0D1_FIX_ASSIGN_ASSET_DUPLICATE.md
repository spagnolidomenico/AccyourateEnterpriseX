# 14.1.0D1 - Fix Assign Asset Duplicate

## Obiettivo

Correggere l'errore di compilazione CS0111 introdotto dalla patch 14.1.0D.

## Errore corretto

```text
AssetManagementView definisce già un membro denominato 'OpenAssignAsset'
con gli stessi tipi di parametro
```

## Correzione

Rimosso l'overload duplicato:

```csharp
OpenAssignAsset(Asset asset)
```

La UI continua a usare il metodo già presente e il metodo interno nullable:

```csharp
OpenAssignAsset(Asset? asset)
```

## Cosa NON cambia

- Nessun database modificato.
- Nessuna modifica login.
- Nessuna modifica branding.
- Nessuna modifica al motore backend.
