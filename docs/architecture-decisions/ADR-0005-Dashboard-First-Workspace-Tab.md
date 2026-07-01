# ADR-0005 - Dashboard come prima Workspace Tab

## Stato

Accettato

## Contesto

La Workspace deve evolvere verso un modello a schede. Per ridurre i rischi, il primo modulo migrato deve essere semplice e non critico.

## Decisione

La Dashboard viene migrata per prima come tab interna.

## Motivazione

La Dashboard è il modulo più adatto per validare il comportamento base delle schede senza toccare processi critici come Digital Twin, AI Assistant o Action Engine.

## Conseguenze

- Il Tab Manager viene usato per la prima volta nel flusso reale.
- La Dashboard diventa il modulo pilota.
- I moduli complessi verranno migrati solo dopo la validazione della Dashboard Tab.
