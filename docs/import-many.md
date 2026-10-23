# Importer plusieurs recettes

> Statut : livré, vérifié sur émulateur — 23/10/2026

## Objectif
Reprendre un carnet de notes de recettes (Google Keep, Samsung Notes, fichiers texte…) d'un seul
coup, au lieu d'importer les recettes une par une avec l'import par IA (`docs/ai-import.md`).

## Comportement attendu
1. Réglages → *Données* → **Importer plusieurs recettes**.
2. « Copier les consignes » : les consignes de l'import par IA (format, unités, rayons, tags),
   suivies de la demande « plusieurs recettes à la suite, un bloc par recette ». L'utilisateur les
   colle dans son IA avec ses notes.
3. « Coller la réponse » : la liste des recettes reconnues s'affiche, chacune avec ses portions,
   le nombre d'ingrédients et d'étapes, et ses **points à vérifier** (avec le numéro de ligne dans
   le texte collé).
4. Toutes sont cochées, sauf :
   - un **titre déjà présent** (dans l'app ou plus haut dans le collage) : signalé, décoché,
     jamais écrasé ;
   - une recette sans titre : signalée, impossible à enregistrer.
5. « Enregistrer la sélection » enregistre les recettes cochées (via `SaveRecipeAsync`) ; elles
   disparaissent de la liste, le compte rendu donne leur nombre.

Le texte peut aussi être écrit à la main au format de Popote : chaque ligne `# Titre` ouvre une
nouvelle recette.

## Fichiers concernés
- `Popote.Data/Services/RecipeTextFormat.cs` — `ParseMany` : découpe sur les lignes `# Titre`
  (pas `##`), ignore le texte avant le premier titre, recale les numéros de ligne.
- `Popote.Tests/RecipeTextFormatTests.cs` — 0, 1, n recettes ; texte avant la première ;
  avertissements rattachés à la bonne recette ; aller-retour avec `Write`.
- `ViewModels/ImportManyViewModel.cs` — copier, coller, enregistrer ; `ImportCandidateViewModel`
  (une recette reconnue, cochable).
- `Views/ImportManyPage.xaml` — consignes, liste des recettes (`BowlCheck`), bouton d'enregistrement.
- `ViewModels/SettingsViewModel.cs`, `Views/SettingsPage.xaml` — entrée dans Réglages.
- `ViewModels/RecipeEditViewModel.cs` — `Describe` (libellé d'un avertissement) partagé.
- `AppShell.xaml.cs`, `MauiProgram.cs` — route et injection.
- `Resources/Strings/AppResources.resx` — `ImportMany_*`, `Import_AskMany`, `Settings_ImportMany*`.

## Choix techniques
- **Même format, même analyseur** : `ParseMany` ne fait que découper puis appelle `Parse` ;
  avertissements, synonymes et tolérances restent identiques.
- Consignes : celles de l'import simple (resx `Import_Instructions`) + une demande différente
  (`Import_AskMany`), plutôt qu'un second gabarit à maintenir.
- Source = texte collé (choix de l'auteur : ses notes sont du texte) : pas de lecture de fichier.
- Doublons comparés sur le titre, sans tenir compte de la casse.

## Modèle de données impacté
Aucun.

## Reste à faire / limites connues
- Pas d'aperçu détaillé ni de correction dans cet écran : corriger après coup dans la recette.
- Un titre presque identique (accent en plus ou en moins) n'est pas vu comme un doublon.
