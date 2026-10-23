# Documentation — Popote

Une feature = un fichier. Pour toute nouvelle feature, créer `docs/<nom>.md`
à partir de `_TEMPLATE.md`, et tenir le fichier à jour quand la feature évolue.

## Design
- [design-system.md](design-system.md) — Système « Mise en place » : métaphore du bol, couleurs testées, typo, jetons, contrôles maison, mouvement, accessibilité. **À suivre pour toute UI.**
- [redesign-foundations.md](redesign-foundations.md) — Refonte, jalon Fondations (1.2.0).
- [redesign-moments.md](redesign-moments.md) — Refonte, jalon Moments (1.3.0) et mesures.
- `licences/` — licences OFL des polices embarquées.

## Développement
- [compiler.md](compiler.md) — Prérequis, compiler, tester, **APK signé** (procédure de livraison), arborescence.

## Features documentées
- [ai-import.md](ai-import.md) — Import par IA : consignes, collage dans le formulaire, format texte (1.4.0).
- [project-setup.md](project-setup.md) — Coquille MAUI compilable (csproj, App, Platforms, Resources).
- [data-migrations.md](data-migrations.md) — Couche données `Popote.Data` & migrations EF Core.
- [tests.md](tests.md) — Tests unitaires (xUnit).
- [recipe-crud.md](recipe-crud.md) — Création / lecture / édition de recettes.
- [recipe-ingredients.md](recipe-ingredients.md) — Édition des ingrédients (nom + quantité + unité).
- [shopping-list.md](shopping-list.md) — Liste de courses agrégée par rayon.
- [recipe-detail.md](recipe-detail.md) — Page détail + portions ajustables (chips, étapes numérotées).
- [recipe-photo.md](recipe-photo.md) — Photo du plat (galerie/appareil photo).
- [recipe-tags.md](recipe-tags.md) — Tags & filtres (puces à bascule, filtre ET).
- [meal-planner.md](meal-planner.md) — Planificateur de repas (semaine → liste de courses).
- [cook-with.md](cook-with.md) — Recherche par ingrédients (« Cuisiner avec… »).
- [cooking-mode.md](cooking-mode.md) — Mode cuisson (étapes défilables, minuteur, écran allumé).
- [favorites-notes.md](favorites-notes.md) — Favoris (★) & notes perso.
- [backup-restore.md](backup-restore.md) — Sauvegarde & restauration des données.

## Repères
- Suivi des modifs au fil de l'eau → `../CHANGELOG.md`
- Objectif et jalons → `../ROADMAP.md`
- Règles de travail → `../CLAUDE.md`
