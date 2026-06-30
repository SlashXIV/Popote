# ROADMAP — Popote

> **Objectif (north star)** : une app Android locale, simple et fiable, pour recenser mes recettes,
> qui me fait progresser sur .NET à parts égales côté données et côté logiciel.
> Toute décision technique sert ce but. En cas de doute : on simplifie.

## Fait
- [x] Squelette : CRUD recettes (titre, portions, préparation), EF Core/SQLite, navigation Shell, MVVM.
- [x] Coquille de projet MAUI compilable (`.csproj`, `App`, `Platforms/Android`, `Resources/`) ; build `net10.0-android` vert. _[DEV]_ — voir `docs/project-setup.md`.
- [x] Édition des ingrédients d'une recette (lignes ingrédient + quantité + unité). _[DATA]_ — voir `docs/recipe-ingredients.md`.
- [x] Page « liste de courses » : sélection multi-recettes → agrégation par rayon. _[DATA]_ — voir `docs/shopping-list.md`.
- [x] Identité : nom **Popote**, logo (monogramme « P » olive + barre or) et design system (base neutre chaude, accent teal). _[DESIGN]_ — voir `docs/design-system.md`.
- [x] Page détail + portions ajustables (chips d'ingrédients, étapes numérotées, recalcul via `ServingsScaler`). _[DEV + DATA]_ — voir `docs/recipe-detail.md`.
- [x] Aide à la saisie d'ingrédient : unité en liste déroulante + suggestions cliquables du catalogue (sans dépendance, plutôt qu'UraniumUI). _[DEV + DATA]_

## En cours
- [ ] (rien pour l'instant)

## Prochain (par priorité)
1. [ ] Photo du plat (`MediaPicker`). _[DEV]_
2. [ ] Tags et filtres (végé, rapide, batch cooking…). _[DATA]_
3. [ ] Édition du rayon des ingrédients (active les pastilles colorées du détail). _[DATA]_

## Plus tard / idées
- [ ] Recherche « qu'est-ce que je peux cuisiner avec les ingrédients X, Y ».
- [ ] Import depuis mes notes existantes.
- [ ] Tests unitaires (au moins `ServingsScaler` et la requête liste de courses).
- [ ] Build Release Android (config trimming pour EF Core).
- [ ] Corriger l'alerte NU1903 : épingler `SQLitePCLRaw.bundle_e_sqlite3` à une version sans vulnérabilité.

## Non-objectifs (pour ne pas se disperser)
- Pas de backend / cloud / compte utilisateur pour l'instant : 100 % local.
- Pas de support iOS tant que la version Android n'est pas stable.
