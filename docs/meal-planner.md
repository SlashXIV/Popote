# Planificateur de repas

> Statut : livré — 2026-07-04

## Objectif
Planifier des recettes sur la semaine et générer la liste de courses
correspondante — la feature qui relie recettes, ingrédients et courses.

## Comportement attendu
- Onglet **« Semaine »** : les 7 jours (lundi → dimanche), navigation ◀ ▶ entre semaines.
- Chaque jour : liste de ses recettes prévues ; **« + Ajouter »** ouvre une feuille
  d'actions listant les recettes ; **✕** retire un repas.
- **« Courses de la semaine »** : agrège les recettes planifiées et affiche la liste
  groupée par rayon, avec cases à cocher (comme l'onglet Courses).

## Fichiers concernés
- `Popote.Data/Models/PlannedMeal.cs` — entité (Date + Recipe) ; migration `AddPlanning`.
- `Popote.Data/Data/AppDbContext.cs` — `DbSet<PlannedMeal>`.
- `Services/RecipeService.cs` — `GetPlannedMealsAsync`, `AddPlannedMealAsync`, `RemovePlannedMealAsync`.
- `ViewModels/PlanningViewModel.cs`, `ViewModels/PlanningDayViewModel.cs`.
- `Views/PlanningPage.xaml(.cs)` ; onglet dans `AppShell.xaml` ; DI dans `MauiProgram.cs`.

## Choix techniques
- **Recettes par jour** (pas de créneau midi/soir) pour le MVP.
- **Liste de courses générée dans le planning** (autonome) : réutilise
  `BuildShoppingListAsync` + l'affichage groupé (`ShoppingAisle` / `ShoppingItemViewModel`).
- Semaine calculée à partir du **lundi** ; libellés via la culture de l'appareil.
- Rendu en **BindableLayout** (pas de `CollectionView` imbriqué) pour cohabiter avec le `ScrollView` de la page.

## Modèle de données impacté
Nouvelle table `PlannedMeal` (FK vers `Recipe`, suppression en cascade). Première
évolution de schéma gérée par **migration EF Core** (voir `docs/data-migrations.md`).

## Reste à faire / limites connues
- Une recette planifiée plusieurs fois dans la semaine n'est comptée **qu'une fois**
  dans les courses (ids distincts) — à affiner si besoin (multiplier les quantités).
- Pas de copie/déplacement d'un repas d'un jour à l'autre, ni de créneaux.
