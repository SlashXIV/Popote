# Import par IA (1.4.0)

> Statut : fait, vérifié sur émulateur — 23/10/2026 (branche `feature/import-ia`)

## Objectif
Créer ou remodeler une recette avec une IA externe (ChatGPT, Claude…) sans rien saisir à la main :
l'app fournit les consignes et le format attendu, l'IA rend un bloc de texte, l'app l'importe.
Aucun réseau, aucune clé : le presse-papiers fait le lien.

## Comportement attendu
Tout se passe dans le formulaire de recette (carte « Avec une IA » en tête), qui sert d'aperçu :
1. « Copier les consignes » : consignes + gabarit + listes autorisées (unités, rayons, tags
   existants) dans le presse-papiers ; si le formulaire a déjà un titre, la recette en cours est
   jointe pour que l'IA la **remodèle**.
2. L'utilisateur colle ces consignes dans l'IA avec sa recette (texte, photo, lien) et copie la réponse.
3. « Coller la réponse » : le formulaire se remplit (photo et favori gardés) ; les points à
   vérifier s'affichent dans la carte et sont lus par TalkBack. Rien de reconnu : le formulaire
   n'est pas touché. Enregistrer passe par `SaveRecipeAsync` (sans migration).
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
- [x] Consignes pour une IA (presse-papiers).
- [x] Collage dans le formulaire, qui sert d'aperçu (choix de l'auteur, plutôt qu'une page dédiée).
- [x] Partage au même format (aller-retour vérifié sur émulateur : partager → copier → coller → enregistrer).

## Fichiers concernés
- `Popote.Data/Services/RecipeVocabulary.cs` — unités et rayons autorisés (source unique ;
  `IngredientLineViewModel.UnitOptions` / `AisleOptions` y renvoient).
- `Popote.Data/Services/RecipeTextFormat.cs` — `Parse` (tolérant, avec avertissements) et `Write`.
- `Popote.Tests/RecipeTextFormatTests.cs` — champs manquants, synonymes, fractions, rayons,
  durées, réponse d'IA bavarde, aller-retour ; le gabarit des consignes (resx) se relit sans
  avertissement.
- `ViewModels/RecipeEditViewModel.cs` — `CopyAiInstructions`, `PasteAiAnswer`, `ImportReport`.
- `Views/RecipeEditPage.xaml` — carte « Avec une IA ».
- `ViewModels/RecipeDetailViewModel.cs` — `Share` écrit le format (aux portions affichées).
- `Resources/Strings/AppResources.resx` — `Import_*` (consignes, compte rendu, avertissements) ;
  `Share_*` retirées.

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
- Le partage n'a plus de signature « partagé depuis Popote » : elle finirait dans les notes.
- Texte ajouté par l'IA après le bloc (« Bon appétit ! ») : rangé dans les notes, à effacer.
- Une quantité écrite en lettres (« deux ») n'est pas reconnue : avertissement, quantité vide.
