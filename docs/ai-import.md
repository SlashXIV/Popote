# Import par IA (1.3.0)

> Statut : en cours — 23/10/2026 (branche `feature/import-ia`)

## Objectif
Créer ou remodeler une recette avec une IA externe (ChatGPT, Claude…) sans rien saisir à la main :
l'app fournit les consignes et le format attendu, l'IA rend un bloc de texte, l'app l'importe.
Aucun réseau, aucune clé : le presse-papiers fait le lien.

## Comportement attendu
1. « Copier les consignes pour une IA » : consignes + gabarit + listes autorisées (unités, rayons,
   tags existants) dans le presse-papiers.
2. L'utilisateur colle ces consignes dans l'IA avec sa recette (texte, photo, lien) et copie la réponse.
3. « Importer une recette » : coller → aperçu (mise en place, étapes, avertissements) → corriger →
   Enregistrer (via `SaveRecipeAsync`, sans migration).
4. « Partager » écrit le même format : une recette partagée se réimporte telle quelle.

Format :
```
# Crêpes de la Chandeleur
Portions : 4
Préparation : 10 min
Cuisson : 20 min
Tags : Dessert, Rapide
## Ingrédients
- 250 g | Farine | Épicerie
- 4 pièce | Œufs | Crèmerie
## Étapes
1. Verser la farine dans un saladier…
## Notes
Moins de sucre la prochaine fois.
```

- [x] Analyseur et écriture du format (`RecipeTextFormat`), testés.
- [ ] Consignes pour une IA (presse-papiers).
- [ ] Écran d'import avec aperçu.
- [ ] Partage au même format.

## Fichiers concernés
- `Popote.Data/Services/RecipeVocabulary.cs` — unités et rayons autorisés (source unique ;
  `IngredientLineViewModel.UnitOptions` / `AisleOptions` y renvoient).
- `Popote.Data/Services/RecipeTextFormat.cs` — `Parse` (tolérant, avec avertissements) et `Write`.
- `Popote.Tests/RecipeTextFormatTests.cs` — champs manquants, synonymes, fractions, rayons,
  durées, réponse d'IA bavarde, aller-retour.

## Choix techniques
- Texte balisé plutôt que JSON : lisible, corrigeable à la main, rien à échapper ; une IA le
  produit sans faute de syntaxe bloquante.
- Analyseur **tolérant** : phrases d'introduction et bloc ```` ``` ```` ignorés, titres `##` ou
  `Ingrédients :`, puces `-` `*` `•` ou numéros, « Personnes » pour « Portions », durées
  « 1 h 30 » / « 1h30 » / « 45 ». Ce qui ne passe pas devient un **avertissement** (avec numéro
  de ligne), jamais une erreur : l'aperçu permet de corriger.
- Unités : synonymes et pluriels ramenés à la liste de l'app (« gr » → « g », « cs » →
  « c. à soupe ») ; une unité inconnue est **gardée** avec un avertissement.
- Rayons : « et » = « & », accents et casse ignorés, quelques synonymes (« Boucherie ») ; un rayon
  inconnu devient « Divers » avec un avertissement.
- Les avertissements sont des codes (`ImportWarningKind`) : le texte affiché vient du resx de
  l'app, la couche données ne porte pas de libellé d'interface.

## Modèle de données impacté
Aucun (pas de migration) : l'import passe par `RecipeService.SaveRecipeAsync`.

## Reste à faire / limites connues
- Une quantité écrite en lettres (« deux ») n'est pas reconnue : avertissement, quantité vide.
