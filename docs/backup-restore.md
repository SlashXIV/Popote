# Sauvegarde & restauration

> Statut : livré — 2026-07-04 ; photos et restauration sûre — 2026-10-23

## Objectif
Popote étant 100 % local, permettre d'**exporter** ses données et de les
**restaurer** (nouveau téléphone, réinstallation, sécurité).

## Comportement attendu
- Page **Réglages** (icône engrenage de l'onglet Recettes), section *Données* — depuis la 1.2.0 ; avant, menu ⋮ :
  - **Sauvegarder** → archive `popote-sauvegarde-AAAAMMJJ-HHmm.zip` : la base `recettes.db3` et les
    photos des recettes (`recipe_*.jpg|png`), puis feuille de partage (Drive, Fichiers, mail…).
  - **Restaurer une sauvegarde** → choisir un `.zip` (ou un ancien `.db3`, toujours accepté) ;
    remplace les données actuelles (confirmation). Redémarrer l'app pour voir le résultat.
- Un fichier qui n'est pas une sauvegarde Popote est **refusé sans rien toucher** (« Ce fichier
  n'est pas une sauvegarde Popote »). Si la restauration ou la migration échoue, la base d'avant
  est remise en place.

## Fichiers concernés
- `Popote.Data/Services/BackupArchive.cs` — écriture de l'archive, validation, restauration,
  retour arrière (`Undo`), réécriture des chemins de photos. Logique pure, testée.
- `Popote.Tests/BackupArchiveTests.cs` — aller-retour base + photos, ancien `.db3`, fichiers
  invalides refusés, `Undo`, entrées d'archive hors photos ignorées.
- `Services/RecipeService.cs` — `CheckpointAsync` (WAL → `.db3`), `GetPhotoPathsAsync`, `MigrateAsync`.
- `ViewModels/SettingsViewModel.cs` — `BackupCommand`, `RestoreCommand` (`Share`, `FilePicker`, alertes).

## Choix techniques
- **`PRAGMA wal_checkpoint(TRUNCATE)`** avant l'export : le journal WAL est rapatrié, la base
  tient en un seul fichier.
- **`System.IO.Compression`** (déjà dans .NET) : aucune dépendance. Photos stockées sans
  compression (JPEG déjà compressé). Seules les photos **référencées** par une recette partent.
- **Validation avant d'écraser** : l'en-tête (`PK\3\4` pour une archive, `SQLite format 3\0` pour
  la base) puis la présence de la table `Recipes`, sur une copie de travail
  (`recettes.restauration.db3`). La base actuelle est copiée en `recettes.avant-restauration.db3`.
- **Chemins de photos** : `PhotoPath` est absolu et propre à l'appareil d'origine ; il est
  réécrit vers le dossier de l'app (seul le nom de fichier compte), dans la copie de travail.
- **Sécurité de l'archive** : seules les entrées `recipe_*.jpg|jpeg|png` sans dossier sont
  extraites (pas de `../`).
- `SqliteConnection.ClearAllPools()` avant le remplacement : une connexion en cache pointerait
  encore sur l'ancien fichier. Connexions de vérification sans pool.

## Reste à faire / limites connues
- Après restauration, un **redémarrage** de l'app est nécessaire (les écrans en cache
  gardent les anciennes données jusqu'au relancement).
- Les photos orphelines de l'appareil (ancienne base) ne sont pas supprimées à la restauration.
