# Favoris & notes

> Statut : livré — 2026-07-04

## Objectif
Marquer ses recettes préférées (★) et garder des notes perso par recette.

## Comportement attendu
- Édition : interrupteur **Favori** + champ **Notes perso**.
- Détail : bouton **★/☆ Favori** (bascule immédiate) + section **Notes** si renseignée.
- Liste : une **★** sur les cartes favorites ; les favoris **remontent en tête**.

## Fichiers concernés
- `Popote.Data/Models/Recipe.cs` — `IsFavorite` (bool) + `Notes` (string?) ; migration `AddFavoriteAndNotes`.
- `Services/RecipeService.cs` — persistance (SaveRecipe), tri favoris d'abord (GetRecipes), `SetFavoriteAsync` (bascule rapide).
- `ViewModels/RecipeEditViewModel.cs`, `RecipeDetailViewModel.cs` ; pages édition / détail / liste.

## Choix techniques
- **Bascule immédiate** au détail via `SetFavoriteAsync` (pas besoin d'ouvrir l'édition).
- **Favoris en tête** : `OrderByDescending(IsFavorite).ThenByDescending(CreatedAt)`.
- Colonnes ajoutées **sans perte de données** grâce aux migrations EF Core.

## Modèle de données impacté
`Recipe` : deux colonnes ajoutées (`IsFavorite`, `Notes`).

## Reste à faire / limites connues
- « Favori » est binaire (pas de note 1–5 étoiles).
- Pas de filtre « uniquement les favoris » (ils sont juste triés en tête).
