# Sauvegarde & restauration

> Statut : livré — 2026-07-04

## Objectif
Popote étant 100 % local, permettre d'**exporter** ses données et de les
**restaurer** (nouveau téléphone, réinstallation, sécurité).

## Comportement attendu
- Menu **⋮** de l'onglet Recettes :
  - **Sauvegarder les données** → exporte la base (`.db3`) et ouvre le partage
    (Google Drive, Fichiers, mail…). Fichier `popote-sauvegarde-AAAAMMJJ-HHmm.db3`.
  - **Restaurer une sauvegarde** → choisir un fichier `.db3` ; remplace les données
    actuelles (confirmation). Redémarrer l'app pour voir le résultat.

## Fichiers concernés
- `Services/RecipeService.cs` — `CheckpointAsync` (WAL → `.db3`), `MigrateAsync`.
- `ViewModels/RecipeListViewModel.cs` — `BackupCommand`, `RestoreCommand` (`Share`, `FilePicker`).
- `Views/RecipeListPage.xaml` — entrées de menu secondaires.

## Choix techniques
- **`PRAGMA wal_checkpoint(TRUNCATE)`** avant l'export : le journal WAL est rapatrié,
  la sauvegarde tient en **un seul fichier**.
- Restauration : on écrase `recettes.db3`, on **supprime les `-wal`/`-shm` périmés**,
  puis on **migre** (au cas où la sauvegarde vient d'une version plus ancienne).
- Partage via `Share` / sélection via `FilePicker` (pickers système, pas de permission spéciale).

## Reste à faire / limites connues
- Les **photos** (fichiers hors base) ne sont pas incluses dans la sauvegarde.
- Après restauration, un **redémarrage** de l'app est nécessaire (les écrans en cache
  gardent les anciennes données jusqu'au relancement).
