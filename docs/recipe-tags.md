# Tags & filtres

> Statut : livré — 2026-07-01

## Objectif
Classer les recettes avec des tags (végé, rapide, dessert…) et filtrer la liste
par tags pour retrouver rapidement une recette.

## Comportement attendu
- Édition : section « Tags » — puces **à bascule** (tap = ajoute/retire le tag) + champ « Nouveau tag » pour en créer un.
- Détail : les tags de la recette s'affichent en puces (lecture seule).
- Liste : une rangée de puces de **filtre** (défilement horizontal) ; activer plusieurs
  tags filtre en **ET** (la recette doit porter *tous* les tags actifs).

## Fichiers concernés
- `Services/RecipeService.cs` — `GetTagsAsync`, filtre par tags dans `GetRecipesAsync`
  (cumul ET), persistance des tags dans `SaveRecipeAsync` (trouver-ou-créer + `GetOrCreateTagAsync`).
- `ViewModels/TagToggleViewModel.cs` — puce à bascule (nom + sélectionné).
- `ViewModels/RecipeEditViewModel.cs` — chargement/sélection des tags, création, sauvegarde.
- `ViewModels/RecipeListViewModel.cs` — tags de filtre + rechargement filtré.
- `ViewModels/RecipeDetailViewModel.cs` — tags en lecture seule.
- `Views/RecipeEditPage.xaml`, `RecipeListPage.xaml`, `RecipeDetailPage.xaml`.

## Choix techniques
- **Relation many-to-many** `RecipeTag` (déjà modélisée, clé composite) ; on remplace
  les liaisons à chaque sauvegarde (clear + recrée), comme pour les ingrédients.
- **Catalogue de tags trouver-ou-créer** insensible à la casse.
- **Filtre ET** : une clause `Where(...Any(tag))` par tag actif (traduit en SQL).
- Chargement des catalogues (ingrédients + tags) via une `Task` attendue par le
  chargement de la recette, pour éviter une course entre les deux.

## Modèle de données impacté
Aucune nouvelle entité : on exploite `Tag` et `RecipeTag` déjà présents.

## Reste à faire / limites connues
- Pas de gestion/renommage/suppression de tags du catalogue (on crée seulement).
- `Tag.Name` n'a pas d'index unique en base (le trouver-ou-créer évite les doublons applicatifs).
