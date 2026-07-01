# Photo du plat

> Statut : livré — 2026-07-01

## Objectif
Associer une photo à une recette, prise depuis la galerie ou l'appareil photo,
et l'afficher dans la liste et la page détail.

## Comportement attendu
- Édition : section « Photo » → boutons **Galerie** / **Appareil photo** ; aperçu
  de l'image + bouton **Retirer**.
- La photo est **copiée dans le dossier privé de l'app** (`FileSystem.AppDataDirectory`) ;
  seul son chemin est stocké (`Recipe.PhotoPath`).
- La photo s'affiche en **miniature** sur la carte de la liste et **en tête** de la page détail.

## Fichiers concernés
- `ViewModels/RecipeEditViewModel.cs` — `PhotoPath` + commandes `PickPhoto` / `TakePhoto` / `RemovePhoto` ; copie du fichier.
- `Views/RecipeEditPage.xaml` — section Photo (aperçu + boutons).
- `ViewModels/RecipeDetailViewModel.cs` / `Views/RecipeDetailPage.xaml` — photo en tête.
- `Views/RecipeListPage.xaml` — miniature sur la carte.
- `Converters/HasTextConverter.cs` — affiche l'image seulement si un chemin est renseigné.
- `Platforms/Android/AndroidManifest.xml` — permission `CAMERA`.

## Choix techniques
- **MediaPicker** (`PickPhotoAsync` / `CapturePhotoAsync`) : API MAUI multiplateforme,
  gère les permissions à l'usage. `IsCaptureSupported` testé avant la capture.
- **Copie en stockage privé** plutôt que garder l'URI temporaire du sélecteur
  (l'URI d'origine n'est pas garanti persistant). Nom de fichier unique (`Guid`).
- **Chemin stocké, pas l'image** en base : `Recipe.PhotoPath` (déjà persisté par `RecipeService`).

## Modèle de données impacté
Aucun changement : `Recipe.PhotoPath` existait déjà.

## Reste à faire / limites connues
- Retirer une photo ne supprime pas le fichier du disque (fichiers orphelins possibles) — nettoyage à prévoir plus tard.
- Pas de recadrage ni de compression (l'image est copiée telle quelle).
