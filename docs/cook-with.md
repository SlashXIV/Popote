# Cuisiner avec… (recherche par ingrédients)

> Statut : livré — 2026-07-04

## Objectif
Répondre à « qu'est-ce que je peux cuisiner avec les ingrédients que j'ai ? » :
cocher des ingrédients et voir les recettes qui les contiennent.

## Comportement attendu
- Accès : bouton **« Cuisiner »** dans la barre d'outils de l'onglet Recettes.
- On coche des ingrédients (puces du catalogue) ; la liste se met à jour en direct.
- Résultat : les recettes contenant **tous** les ingrédients cochés (cumul ET) ;
  tap sur une recette → sa page détail.

## Fichiers concernés
- `Services/RecipeService.cs` — `FindRecipesByIngredientsAsync` (une clause `Where(...Any())` par ingrédient).
- `ViewModels/CookWithViewModel.cs` — puces d'ingrédients + résultats.
- `Views/CookWithPage.xaml(.cs)` — page (chips + liste de résultats).
- `AppShell.xaml.cs` (route), `MauiProgram.cs` (DI), `RecipeListPage.xaml` (bouton d'accès).

## Choix techniques
- **Cumul ET** (cohérent avec le filtre par tags) : la recette doit contenir chaque
  ingrédient coché. Requête traduite en SQL par EF Core.
- Puces à bascule réutilisant `TagToggleViewModel` (nom + sélectionné).

## Modèle de données impacté
Aucun (lecture seule sur les entités existantes).

## Reste à faire / limites connues
- Sémantique « ET » : ne propose pas les recettes « presque faisables » ni le
  classement par nombre d'ingrédients correspondants (piste : mode OU / faisabilité).
