# Tests unitaires

> Statut : livré — 2026-07-04

## Objectif
Un filet de sécurité sur la logique métier pure, et un point d'entrée pour
apprendre les tests en .NET.

## Ce qui est testé
- `ServingsScaler.Scale` : mise à l'échelle des quantités (proportion, portions
  égales, garde-fou division par zéro).
- `ShoppingListBuilder.Aggregate` : regroupement/somme des ingrédients (même
  unité sommée, unités différentes séparées, rayon absent → « Divers », tri).

## Où
- Projet `Popote.Tests` (xUnit, `net10.0`) référence `Popote.Data`.
- Pour être testable depuis un projet desktop, la **logique pure** a été extraite
  dans `Popote.Data/Services/` (`ServingsScaler`, `ShoppingListBuilder`) — hors du
  projet app Android. `RecipeService.BuildShoppingListAsync` délègue à `ShoppingListBuilder`.

## Lancer les tests
```sh
dotnet test Popote.Tests/Popote.Tests.csproj
```

## Reste à faire / limites connues
- Pas encore de tests sur les requêtes EF (possible via SQLite en mémoire).
- Pas de tests d'UI / ViewModels.
