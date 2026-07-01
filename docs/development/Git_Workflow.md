# Git Workflow - Accyourate Enterprise X

## Branch principali

```text
main
develop
feature/*
fix/*
release/*
```

## Flusso standard feature

```powershell
git checkout develop
git pull
git checkout -b feature/nome-feature
```

Dopo sviluppo e test:

```powershell
git status
git add .
git commit -m "UI-003: Short description"
git push -u origin feature/nome-feature
```

Poi aprire Pull Request verso `develop`.

## Release stabile

Quando `develop` è validato:

```powershell
git checkout main
git merge develop
git tag -a vX.Y.Z-stable -m "Release X.Y.Z Stable"
git push origin main
git push origin vX.Y.Z-stable
```

## Regole

- `main` deve sempre compilare.
- ogni tag deve puntare a una versione validata.
- ogni sprint deve avere checklist.
