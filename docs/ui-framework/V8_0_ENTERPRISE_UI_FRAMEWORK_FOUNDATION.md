# Accyourate Enterprise X - Versione 8.0 Enterprise UI Framework Foundation

## Obiettivo

Stabilizzare il metodo di sviluppo UI e preparare la migrazione progressiva verso un'interfaccia enterprise coerente.

## Decisione tecnica

Da questa versione:

- le nuove interfacce dovranno essere progettate in modalità XAML-first;
- il C# dovrà contenere logica, binding e orchestrazione, non layout complessi;
- i componenti visuali saranno riutilizzabili;
- ogni modulo verrà migrato progressivamente nella shell.

## Aggiunto

- `UIFramework/Tokens`
- `UIFramework/Components`
- `UIFramework/Shell`
- `UIFramework/Themes`
- `IShellModule`
- `ShellRegistry`
- `EnterpriseShellFoundationWindow`

## Template XAML

Sono presenti template `.axaml.template` per:

- colori;
- pulsanti;
- card.

Sono intenzionalmente template e non ancora collegati al progetto per evitare regressioni di compilazione durante la fase foundation.

## Roadmap

- 8.1 Enterprise Shell con area contenuti reale
- 8.2 Dashboard refactor
- 8.3 Medical Suite refactor
- 8.4 Document Management refactor
- 8.5 Theme/Branding integrati
