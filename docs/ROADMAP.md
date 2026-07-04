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
- [x] Édition du rayon des ingrédients (Picker par ligne) → pastilles colorées du détail + meilleur regroupement des courses. _[DATA]_
- [x] Liste de courses actionnable : cases à cocher pour barrer les articles. _[DEV]_
- [x] Photo du plat (galerie/appareil photo via `MediaPicker`) : miniature en liste, photo en détail. _[DEV]_ — voir `docs/recipe-photo.md`.
- [x] Tags et filtres (puces à bascule + filtre ET). _[DATA]_ — voir `docs/recipe-tags.md`.
- [x] Suppression d'une recette depuis l'UI (balayage + confirmation). _[DEV]_
- [x] Temps de prépa / cuisson : édition + affichage au détail. _[DEV + DATA]_
- [x] Migrations EF Core (couche données `Popote.Data`, remplace `EnsureCreated`). _[DATA]_ — voir `docs/data-migrations.md`.
- [x] Planificateur de repas (semaine → liste de courses). _[DATA + DEV]_ — voir `docs/meal-planner.md`.
- [x] Tests unitaires (ServingsScaler + agrégation des courses, xUnit). _[DEV]_ — voir `docs/tests.md`.
- [x] Nettoyage des API MAUI dépréciées (`DisplayAlertAsync`, `PickPhotosAsync`…). _[DEV]_
- [x] Recherche par ingrédients (« cuisiner avec X, Y », cumul ET). _[DATA]_ — voir `docs/cook-with.md`.
- [x] Mode cuisson : étapes défilables, écran maintenu allumé, minuteur. _[DEV]_ — voir `docs/cooking-mode.md`.
- [x] Favoris (★) + notes perso par recette. _[DATA + DEV]_ — voir `docs/favorites-notes.md`.

## En cours
- [ ] (rien pour l'instant)

> 🎉 Toutes les priorités **initiales** sont livrées. Nouvelle vague ci-dessous.

## Prochain (par priorité)
- [ ] (à définir — voir « Plus tard / idées »)

## Plus tard / idées
- [ ] Liste de courses : ajouter un article manuel + persister les cases cochées.
- [ ] Tri de la liste (date / titre / temps).
- [ ] Import d'une recette (URL ou texte collé) ; import de mes notes existantes.
- [ ] Export / partage d'une recette (texte) + sauvegarde/restauration du `.db3`.
- [ ] Corriger l'alerte NU1903 (`SQLitePCLRaw` transitif) quand une version corrigée sera tirable par EF Core.
- [ ] Build Release Android signé (APK installable sans PC ; trimming EF Core).

## Non-objectifs (pour ne pas se disperser)
- Pas de backend / cloud / compte utilisateur pour l'instant : 100 % local.
- Pas de support iOS tant que la version Android n'est pas stable.
