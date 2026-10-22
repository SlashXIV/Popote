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
- [x] Build Release Android **signé** (APK autonome ; trimming off pour EF Core). _[DEV]_ — voir `README.md`.
- [x] Publication GitHub + **Release** (APK via lien `latest`). _[DEV]_
- [x] Sauvegarde / restauration de la base (`.db3` partageable + import). _[DATA + DEV]_ — voir `docs/backup-restore.md`.
- [x] Liste de courses **persistante** : article manuel + cases cochées persistées + retrait/vidage. _[DATA]_ — voir `docs/shopping-list.md`.
- [x] Partage d'une recette en texte (feuille de partage système). _[DEV]_
- [x] Tri de la liste des recettes (Favoris / Récent / Titre / Temps). _[DATA]_
- [x] **Refonte « Mise en place » — Fondations (1.1.0)** : métaphore du bol, base safran/aubergine, thème sombre, Réglages, typo, jetons, contrôles maison, textes externalisés, logo. _[DESIGN + DEV]_ — voir `docs/redesign-foundations.md` et `docs/design-system.md`.

## En cours
- [ ] **Refonte — Moments (1.2.0)** : la mise en place (ouverture d'une recette), le passage au poste, l'étape suivante, le minuteur fini, l'article coché, la recette enregistrée, transitions de page. _[DESIGN + DEV]_
- [ ] **Vitrine** : README illustré (logo + 4 captures). _[DESIGN]_

### Refonte — preuves
| Jalon | Version | Preuve |
|---|---|---|
| Fondations | 1.1.0 | 50 tests verts (dont contrastes/daltonisme des 2 thèmes, aucun texte en dur) ; build sans avertissement nouveau ; émulateur : 8 pages en clair et en sombre, police 200 %, densité 600, arbre TalkBack ; téléphone (Release) : démarrage 1,76 s, liste p90 7 ms, carrousel p90 5 ms |

### Journal des décisions (refonte)
| Décision | Raison |
|---|---|
| Métaphore unique : la mise en place (le bol) | la cohérence d'une métaphore, pas la quantité d'effets, fait l'identité |
| Planche validée avant le code | on juge l'ensemble avant de coder écran par écran |
| Base safran + aubergine (pas blanc/vert) | demande explicite : plus « pétillant » |
| Émaux de rayon calculés et testés | donnée codée par la couleur : 3:1 + distincts en protan/deutan |
| Aucune dépendance d'interface | MAUI + contrôles maison suffisent ; pas de lock-in |
| Barre d'onglets Shell gardée, rendu maison | navigation, retour et TalkBack natifs gratuits |
| `Bowl` / `Icon` en GraphicsView | mesuré : les formes MAUI empilées coûtaient des images au défilement |
| Release sans AOT | AOT sans trimming = plantage au démarrage ; trimming exclu pour EF Core |
| Mesures sur téléphone uniquement | l'émulateur varie trop d'un passage à l'autre pour comparer |

## Plus tard / idées
- [ ] Import d'une recette (URL ou texte collé) ; import de mes notes existantes.
- [ ] Corriger l'alerte NU1903 (`SQLitePCLRaw` transitif) quand une version corrigée sera tirable par EF Core.

## Non-objectifs (pour ne pas se disperser)
- Pas de backend / cloud / compte utilisateur pour l'instant : 100 % local.
- Pas de support iOS tant que la version Android n'est pas stable.
