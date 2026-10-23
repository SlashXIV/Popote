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
- `StepParser` : découpage des étapes.
- `RecipeTextFormat` (1.4.0) : format texte de l'import par IA ; `ParseMany` (1.5) : plusieurs
  recettes à la suite, numéros de ligne dans le texte entier.
- `BackupArchive` (1.5) : sauvegarde `.zip` aller-retour (base + photos, chemins réécrits), ancien
  `.db3` accepté, fichiers invalides refusés sans toucher la base, `Undo`, entrées d'archive hors
  photos ignorées — sur de vraies bases SQLite dans un dossier temporaire.
- **Interface (depuis la 1.2.0)** :
  - `ThemeContrastTests` : relit `AppColors.xaml` ; contrastes WCAG (texte 7:1, accents 4,5:1,
    graphiques 3:1) dans les deux thèmes, émaux de rayon distincts en protan / deutan
    (Machado 2009, ΔE OKLab ≥ 8), chaque rôle présent en clair et en sombre.
  - `HardcodedUiTextTests` : aucun texte d'interface en dur dans `Views/*.xaml` ni dans le C#
    d'interface (exception : commentaire « texte-technique : <raison> »).
  - `ResxTests` : chaque clé utilisée existe, aucune clé orpheline, typographie française.
  - `MotionTests` : niveaux de mouvement, courbes de Bézier (bornes, monotonie, rebond).
  - `QuantityFormatTests` : pluriels, virgule décimale, espace insécable, lecture TalkBack.

## Où
- Projet `Popote.Tests` (xUnit, `net10.0`) référence `Popote.Data`.
- Pour être testable depuis un projet desktop, la **logique pure** a été extraite
  dans `Popote.Data/Services/` (`ServingsScaler`, `ShoppingListBuilder`) — hors du
  projet app Android. `RecipeService.BuildShoppingListAsync` délègue à `ShoppingListBuilder`.
- Le code de présentation pur (`Motion/CubicBezier.cs`, `Motion/MotionLevel.cs`,
  `Services/QuantityFormat.cs`) est **lié** dans le projet de test ; XAML et resx sont relus
  comme du XML depuis la racine du dépôt (`RepoFiles`).

## Lancer les tests
```sh
dotnet test Popote.Tests/Popote.Tests.csproj
```

## Reste à faire / limites connues
- Pas encore de tests sur les requêtes EF (possible via SQLite en mémoire) ; `BackupArchive` passe
  par `Microsoft.Data.Sqlite` directement.
- Pas de tests d'UI / ViewModels.
