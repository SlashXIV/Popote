# Édition des ingrédients d'une recette

> Statut : livré — 2026-06-27

## Objectif
Permettre de saisir les ingrédients d'une recette sous forme de lignes
`nom + quantité + unité`, et de les conserver en base. C'est le socle des
features suivantes (liste de courses, portions ajustables).

## Comportement attendu
- Dans la page d'édition, une section « Ingrédients » liste les lignes existantes.
- « Ajouter un ingrédient » crée une ligne vide à remplir.
- Chaque ligne a un bouton ✕ pour la supprimer.
- « Enregistrer » persiste les lignes ; au rechargement de la recette, elles réapparaissent.
- Les lignes sans nom sont ignorées. La quantité accepte la virgule ou le point.

## Fichiers concernés
- `Services/RecipeService.cs` — `SaveRecipeAsync(Recipe, IReadOnlyList<IngredientInput>)`
  remplace l'ancienne signature ; gère le catalogue via `GetOrCreateIngredientAsync`.
  Nouveau DTO `IngredientInput(Name, Quantity, Unit)`.
- `ViewModels/IngredientLineViewModel.cs` — ligne éditable (Nom, QuantityText, Unit).
- `ViewModels/RecipeEditViewModel.cs` — collection `Ingredients`, commandes
  `AddIngredient` / `RemoveIngredient`, chargement et parsing de la quantité.
- `Views/RecipeEditPage.xaml` — section « Ingrédients » (BindableLayout + bouton ajouter).

## Choix techniques
- **Catalogue trouver-ou-créer, insensible à la casse** : « Tomate » et « tomate »
  pointent vers le même `Ingredient`, pour éviter les doublons.
- **Remplacement intégral des lignes à la sauvegarde** (clear + recrée) plutôt que
  diff fin : simple et fiable pour un usage perso ; EF supprime les `RecipeIngredient`
  orphelins par cascade. Le catalogue `Ingredient`, lui, n'est jamais supprimé.
- **Quantité saisie en texte** (`QuantityText`) puis parsée à l'enregistrement
  (virgule → point, `InvariantCulture`) : évite les soucis de culture du binding.
- **Unité choisie dans une liste** (`Picker`, `IngredientLineViewModel.UnitOptions`)
  plutôt qu'en texte libre : évite les doublons (« g » / « gr ») qui casseraient l'agrégation des courses.
- **Suggestions du catalogue** (`KnownIngredients`, chips « Déjà utilisés ») : tap = ligne pré-remplie.
  Choix d'une solution maison sans dépendance plutôt qu'UraniumUI (qui imposait de monter MAUI à 10.0.71).
- **`BindableLayout`** plutôt qu'un `CollectionView` : peu de lignes, pas besoin de
  virtualisation ; la suppression vise la commande du VM de page via `x:Reference`.

## Modèle de données impacté
Aucune nouvelle entité : on exploite `RecipeIngredient` (Quantity, Unit) et le
catalogue `Ingredient` (Name unique) déjà présents.

## Reste à faire / limites connues
- Le **rayon** (`Ingredient.Aisle`) n'est pas éditable ici (décidé : plus tard, avec
  la liste de courses). Les nouveaux ingrédients ont donc `Aisle = null`.
- Pas d'autocomplétion sur les noms d'ingrédients déjà connus (amélioration possible).
- Renommer un ingrédient existant via une recette crée un nouvel ingrédient de
  catalogue plutôt que de renommer l'ancien (comportement voulu pour l'instant).
