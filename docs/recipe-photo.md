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
- `Services/PhotoThumbnails.cs` — miniature `recipe_x.min.jpg` (360 px sur le petit côté, JPEG 80),
  créée à la copie de la photo et, en arrière-plan au démarrage, pour les photos qui n'en ont pas.
- `Converters/PhotoThumbConverter.cs` — la liste affiche la miniature si elle existe, sinon l'original.
- `Platforms/Android/AndroidManifest.xml` — permission `CAMERA`.

## Choix techniques
- **MediaPicker** (`PickPhotoAsync` / `CapturePhotoAsync`) : API MAUI multiplateforme,
  gère les permissions à l'usage. `IsCaptureSupported` testé avant la capture.
- **Copie en stockage privé** plutôt que garder l'URI temporaire du sélecteur
  (l'URI d'origine n'est pas garanti persistant). Nom de fichier unique (`Guid`).
- **Chemin stocké, pas l'image** en base : `Recipe.PhotoPath` (déjà persisté par `RecipeService`).
- **Miniatures (1.5)** : la liste n'affiche que 64 dp ; la miniature est un fichier à côté de
  l'original (aucune colonne, aucune migration). Décodage Android réduit (`InSampleSize`) : l'original
  n'est jamais chargé en entier ; orientation EXIF appliquée (`BitmapFactory` l'ignore). Le détail
  garde l'original (photo pleine largeur). Les miniatures ne partent pas dans la sauvegarde : elles
  sont recréées au démarrage suivant la restauration.

## Modèle de données impacté
Aucun changement : `Recipe.PhotoPath` existait déjà.

## Reste à faire / limites connues
- Retirer une photo ne supprime pas le fichier du disque (fichiers orphelins possibles) — nettoyage à prévoir plus tard.
- Pas de recadrage ni de compression de l'original (copié tel quel) ; seule la liste profite de la miniature.
- Orientations EXIF « miroir » (rares) traitées comme normales dans la miniature.
- **Mémoire : pas de gain** (téléphone, Release, `bench.sh`) : PSS de la liste 359–364 Mo en 1.4.1,
  362–366 Mo en 1.5.0. MAUI (Glide) réduisait déjà l'image à la taille de la vignette ; même build
  avec et sans photos : 373–378 contre 372–373 Mo. Les ~360 Mo sont le runtime .NET sans AOT (tas
  natif ≈ 123 Mo) et les tampons graphiques (≈ 93 Mo), pas les photos. Gain mesuré : défilement
  p90 12 → 10 ms (image 18 Ko au lieu d'1 Mo à décoder). Orientation EXIF vérifiée sur le Xiaomi.
