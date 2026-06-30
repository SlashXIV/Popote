# Changelog

Toutes les modifications notables du projet sont consignées ici.
Format inspiré de [Keep a Changelog](https://keepachangelog.com/fr/1.1.0/),
versionnage [SemVer](https://semver.org/lang/fr/).

## [Unreleased]

### Added
- Squelette initial : CRUD recettes (titre, portions, préparation).
- Couche données EF Core / SQLite : entités `Recipe`, `Ingredient`, `RecipeIngredient`, `Tag`, `RecipeTag`.
- `RecipeService` : CRUD + génération de liste de courses agrégée (`GroupBy` + `Sum`).
- `ServingsScaler` : mise à l'échelle des portions.
- Navigation Shell, injection de dépendances et création de la base au démarrage (`MauiProgram`).
- **Coquille de projet MAUI compilable** : `RecettesApp.csproj` (cible `net10.0-android`),
  classe `App`, `Platforms/Android`, `Resources/` (polices, icône, splash, styles).
  Le projet compile désormais (`dotnet build -f net10.0-android` : 0 erreur).
- README racine : prérequis et procédure de build documentés.

- Édition des ingrédients d'une recette : lignes `nom + quantité + unité`,
  ajout/suppression, persistées via `RecipeService`. Catalogue d'ingrédients
  trouver-ou-créer insensible à la casse.
- Liste de courses : onglet « Courses », sélection multi-recettes → liste agrégée
  regroupée par rayon (réutilise `BuildShoppingListAsync`). Navigation par onglets (`TabBar`).
- Design system (brief `docs/design-system.md`) : base neutre chaude + accents
  « earthy naturals » (teal principal) en tokens (`AppColors.xaml`), styles de
  composants (`AppStyles.xaml`), police Inter, thème clair forcé. Chrome dé-violetté
  (Shell + barre système + token `Primary` du template + splash/icône). Appliqué aux
  pages liste, courses et édition.

### Changed
- Code source rangé en dossiers : `Models/`, `Data/`, `Services/`, `ViewModels/`, `Views/`.
- `RecipeService.SaveRecipeAsync` prend désormais aussi les lignes d'ingrédients
  (`IReadOnlyList<IngredientInput>`).

### Security
- Avertissement connu NU1903 sur `SQLitePCLRaw.lib.e_sqlite3.android` 2.1.11 (transitif EF Core) — correctif à venir.
