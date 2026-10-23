# Mode cuisson

> Statut : livré — 2026-07-04

## Objectif
Accompagner la cuisine : suivre les étapes une par une, sans que l'écran
s'éteigne, avec un minuteur à portée de main.

## Comportement attendu
- Accès : bouton **« Cuisiner »** sur la page détail d'une recette.
- Depuis la 1.2.0 (« le poste de travail ») : numéro d'étape en très grand, texte en 23, rangée de bols de progression, boutons « Précédente » / « Étape suivante » de 64 de haut, minuteur en chiffres tabulaires ; une seule annonce TalkBack à la fin du minuteur, plus une vibration (réglable).
- Étapes **défilables** (glisser horizontalement, `CarouselView` + points).
- **Écran maintenu allumé** tant qu'on est sur la page.
- **Minuteur** : saisir des minutes → décompte `mm:ss` ; alerte à la fin.

## Fichiers concernés
- `Popote.Data/Services/StepParser.cs` — parsing des étapes (partagé avec le détail, testé).
- `ViewModels/CookingModeViewModel.cs` — étapes + minuteur (`IDispatcherTimer`).
- `Views/CookingModePage.xaml(.cs)` — UI ; `KeepScreenOn` géré dans le code-behind.
- `AppShell.xaml.cs` (route), `MauiProgram.cs` (DI), `RecipeDetailPage.xaml` (bouton).

## Choix techniques
- **`DeviceDisplay.KeepScreenOn`** activé à l'affichage, désactivé en quittant (code-behind).
- **`IDispatcherTimer`** (dispatcher UI) pour le décompte ; arrêté en quittant la page.
- **`StepParser`** extrait dans la lib → réutilisé par le détail et le mode cuisson, et **testé** unitairement.

## Modèle de données impacté
Aucun.

## Reste à faire / limites connues
- Minuteur **manuel** (pas de durée par étape : les étapes n'ont pas de temps associé).
- Un seul minuteur à la fois.
