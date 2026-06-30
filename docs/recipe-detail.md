# Page détail + portions ajustables

> Statut : livré — 2026-06-30

## Objectif
Offrir une page de **consultation** d'une recette (lecture seule), mettant en
valeur le design, et permettant d'**ajuster les portions** pour recalculer
automatiquement les quantités d'ingrédients.

## Comportement attendu
- Tap sur une recette (liste) → page détail (l'édition se fait via « Éditer »).
- Sélecteur de **portions** (stepper) : le changer recalcule les quantités via `ServingsScaler`.
- **Ingrédients en chips** : nom + quantité recalculée + unité, avec une **pastille de rayon** colorée.
- **Préparation en étapes numérotées** : une ligne non vide = une étape.
- Bouton **« Éditer »** (barre d'outils) → page d'édition.

## Fichiers concernés
- `ViewModels/RecipeDetailViewModel.cs` — charge la recette, recalcule les quantités
  quand `TargetServings` change ; expose `Ingredients` (records `ScaledIngredient`) et `Steps` (`StepLine`).
- `Views/RecipeDetailPage.xaml(.cs)` — UI (chips + étapes numérotées) au design system.
- `Converters/AisleToColorConverter.cs` — rayon → couleur de pastille (palette « earthy naturals »).
- `ViewModels/RecipeListViewModel.cs` — le tap ouvre désormais le détail (`GoToDetailCommand`).
- `AppShell.xaml.cs` (route `RecipeDetailPage`), `MauiProgram.cs` (DI).

## Choix techniques
- **Lecture seule + recalcul à la volée** : on garde les quantités de base en mémoire
  et on applique `ServingsScaler.Scale(base, portionsBase, portionsCibles)` à chaque changement.
- **Étapes = découpage par lignes** de `Instructions` (pas de nouveau champ en base).
- **Pastille de rayon** via converter ; rayon non encore saisi → gris « Divers »
  (se colorera quand l'édition du rayon existera).
- **Aucun changement du modèle de données** : `ServingsScaler` et `Recipe.Servings` existaient déjà.

## Modèle de données impacté
Aucun.

## Reste à faire / limites connues
- Le rayon n'est pas encore éditable → pastilles grises pour l'instant.
- Quantités recalculées affichées avec au plus 2 décimales (séparateur point).
- Pas de photo ni de temps de préparation affichés (viendront avec leurs features).
