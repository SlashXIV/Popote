# Recipe CRUD — gestion des recettes

> Statut : livré (squelette) — base initiale

## Objectif
Créer, lister, consulter et modifier des recettes stockées localement.

## Comportement attendu
- Liste des recettes avec recherche par titre et pull-to-refresh.
- Bouton « Ajouter » → formulaire (titre, portions, préparation).
- Tap sur une recette → édition du même formulaire.
- « Enregistrer » → retour à la liste, qui se rafraîchit.

## Fichiers concernés
- `Models/Recipe.cs` — entité recette.
- `Data/AppDbContext.cs` — tables et configuration EF Core.
- `Services/RecipeService.cs` — requêtes (`GetRecipesAsync`, `GetRecipeAsync`, `SaveRecipeAsync`, `DeleteRecipeAsync`).
- `ViewModels/RecipeListViewModel.cs`, `ViewModels/RecipeEditViewModel.cs`.
- `Views/RecipeListPage.xaml(.cs)`, `Views/RecipeEditPage.xaml(.cs)`.

## Choix techniques
- EF Core via `IDbContextFactory` (DbContext court par opération) plutôt qu'un contexte partagé.
- `EnsureCreated()` au démarrage (pas de migrations) : suffisant en phase d'apprentissage.

## Modèle de données impacté
- Entité `Recipe`, avec relations vers `RecipeIngredient` et `RecipeTag` (exploitées plus tard).

## Reste à faire / limites connues
- L'édition ne gère pas encore les ingrédients ni les tags (voir ROADMAP).
- Pas de suppression depuis l'UI pour l'instant (la méthode service existe).
