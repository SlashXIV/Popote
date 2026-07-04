# Liste de courses

> Statut : livré — 2026-06-28

## Objectif
Sélectionner plusieurs recettes et obtenir une liste de courses agrégée :
les mêmes ingrédients sont fusionnés et leurs quantités additionnées, le tout
regroupé par rayon pour faciliter les achats.

## Comportement attendu
- Onglet « Courses » (en bas de l'app).
- En haut : la liste des recettes, chacune avec une case à cocher.
- Bouton « Générer la liste ».
- En bas : la liste agrégée, regroupée par rayon (en-têtes), chaque ligne
  affichant le nom et la quantité cumulée (+ unité).
- La liste est **persistante** : « Générer » (re)construit les articles issus des
  recettes en **conservant les articles ajoutés à la main**.
- **Ajout manuel** d'un article (ex. « sopalin »), **retrait** (✕) et **vidage** de la liste.
- Chaque article a une **case à cocher** ; l'état **persiste** (survit au redémarrage).

## Fichiers concernés
- `Services/RecipeService.cs` — `BuildShoppingListAsync` (déjà écrit : `GroupBy` +
  `Sum`, tri par rayon puis nom) ; `ShoppingItem` reçoit un libellé d'affichage `QuantityLabel`.
- `ViewModels/SelectableRecipeViewModel.cs` — enveloppe une recette + état « coché ».
- `ViewModels/ShoppingListViewModel.cs` — chargement, sélection, génération ;
  contient `ShoppingAisle` (groupe par rayon pour le CollectionView).
- `Views/ShoppingListPage.xaml(.cs)` — page unique (sélection + résultat groupé).
- `AppShell.xaml` — `TabBar` à deux onglets (« Recettes », « Courses »).
- `MauiProgram.cs` — enregistrement DI de la page et du ViewModel.

## Choix techniques
- **Page unique** (sélection + résultat) plutôt que deux pages : feedback immédiat, plus simple.
- **Onglets Shell (`TabBar`)** pour la navigation principale.
- **Wrapper `SelectableRecipeViewModel`** (Recipe + IsSelected) plutôt que la
  sélection native du `CollectionView` : binding MVVM clair, testable.
- **`CollectionView IsGrouped="True"`** alimenté par des `ShoppingAisle : List<ShoppingItem>`
  (modèle de groupe attendu par MAUI). L'ordre des groupes suit le tri du service.
- **Agrégation côté service** (réutilisée telle quelle) : le ViewModel ne fait que
  collecter les ids cochés et regrouper le résultat pour l'affichage.

## Modèle de données impacté
Table **`ShoppingListItem`** (persistante) : nom, rayon, quantité, unité,
`IsChecked`, `IsManual`. `RebuildShoppingListAsync` remplace les articles auto
(garde les manuels) ; `SetItemCheckedAsync` persiste la coche.

## Reste à faire / limites connues
- Le **rayon** vient de `Ingredient.Aisle` ; les ingrédients sans rayon tombent dans « Divers ».
- Pas de **mise à l'échelle des portions** dans l'agrégation (quantités de base).
- « Générer » **réinitialise** les articles auto (leur coche est perdue) ; les articles manuels et leur coche sont conservés.
