# Git Branch Strategy

## Branch principali

- `main`: solo versioni stabili.
- `develop`: sviluppo corrente validato.
- `feature/*`: lavori specifici.

## Flusso consigliato

```text
feature/workspace-module-registry
        ↓
develop
        ↓
main
```

## Regole

- Non sviluppare direttamente su `main`.
- Ogni feature deve avere un commit chiaro.
- Ogni sprint validato va pushato su `develop`.
- `main` riceve solo milestone stabili.

## Tag consigliati

- `v11.0.5`
- `v11.1.0-rc1`
- `v11.1.0`
- `v12.0.0-beta`
