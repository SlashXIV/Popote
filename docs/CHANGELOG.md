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
- **Coquille de projet MAUI compilable** : `Popote.csproj` (cible `net10.0-android`),
  classe `App`, `Platforms/Android`, `Resources/` (polices, icône, splash, styles).
  Le projet compile désormais (`dotnet build -f net10.0-android` : 0 erreur).
- README racine : prérequis et procédure de build documentés.

- Édition des ingrédients d'une recette : lignes `nom + quantité + unité`,
  ajout/suppression, persistées via `RecipeService`. Catalogue d'ingrédients
  trouver-ou-créer insensible à la casse.
- Liste de courses : onglet « Courses », sélection multi-recettes → liste agrégée
  regroupée par rayon (réutilise `BuildShoppingListAsync`). Navigation par onglets (`TabBar`).
- Page détail (consultation) avec **portions ajustables** : recalcul des quantités
  via `ServingsScaler`, ingrédients en chips (pastille de rayon), préparation en étapes numérotées.
- Détail : **multiplicateur de portions** (×½ ×1 ×2 ×3) relatif à la recette de base, plus intuitif.
- Édition : **unité d'ingrédient via liste déroulante** (Picker) au lieu de texte libre (évite les doublons d'unités).
- Édition : **suggestions cliquables du catalogue** (« Déjà utilisés ») pour ajouter vite un ingrédient connu et favoriser la réutilisation.
- Édition : **rayon de l'ingrédient** (Picker par ligne) — colore les pastilles du détail et affine le regroupement de la liste de courses. Le rayon est mémorisé sur l'ingrédient du catalogue.
- Liste de courses : **cases à cocher** pour barrer les articles au fur et à mesure des courses (état éphémère, réinitialisé à chaque génération).
- **Photo du plat** : ajout via galerie ou appareil photo (`MediaPicker`), copiée en stockage privé ; miniature en liste, photo en tête du détail.
- **Tags & filtres** : tags à bascule sur une recette (+ création), affichés au détail, et filtre de la liste par tags (cumul ET).
- **Suppression d'une recette** depuis la liste : balayage vers la gauche + confirmation.
- **Temps de préparation / cuisson** : éditables sur la recette, affichés au détail (« Prépa 15 min · Cuisson 20 min »).
- Design system (brief `docs/design-system.md`) : base neutre chaude + accents
  « earthy naturals » (teal principal) en tokens (`AppColors.xaml`), styles de
  composants (`AppStyles.xaml`), police Inter, thème clair forcé. Chrome dé-violetté
  (Shell + barre système + token `Primary` du template + splash/icône). Appliqué aux
  pages liste, courses et édition.

### Changed
- Code source rangé en dossiers : `Models/`, `Data/`, `Services/`, `ViewModels/`, `Views/`.
- `RecipeService.SaveRecipeAsync` prend désormais aussi les lignes d'ingrédients
  (`IReadOnlyList<IngredientInput>`).

### Fixed
- Violet résiduel du template MAUI : les tokens `Primary`/`Secondary`/`Tertiary`
  de `Colors.xaml` sont ramenés au teal, donc les contrôles non stylés (boutons ✕,
  « Ajouter un ingrédient »…) ne sont plus violets.
- Chips « Déjà utilisés » tronqués (« Cacao » → « Caca ») : plus de rétrécissement dans le FlexLayout.
- Double numérotation des étapes de préparation (badge + « 1. » déjà tapé) : la numérotation en tête de ligne est retirée.
- Croix de suppression d'ingrédient allégée (✕ gris discret au lieu d'un carré plein).
- Noms d'ingrédients normalisés avec majuscule initiale (« levure » → « Levure »).
- En-tête de rayon coloré (pastille) dans la liste de courses.

### Security
- Avertissement connu NU1903 sur `SQLitePCLRaw.lib.e_sqlite3.android` 2.1.11 (transitif EF Core) — correctif à venir.
