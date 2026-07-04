# Couche données & migrations EF Core

> Statut : livré — 2026-07-04

## Objectif
Faire évoluer le schéma de la base **sans perdre les données** (indispensable dès
qu'on ajoute une table/colonne, ex. le futur planificateur), via les migrations EF Core.

## Pourquoi une bibliothèque `Popote.Data`
L'outil `dotnet ef` ne peut pas s'exécuter sur un projet **Android seul** (pas d'hôte
exécutable → « deps.json does not exist »). On a donc déplacé la couche données dans une
**bibliothèque de classes `net10.0`** (`Popote.Data`), sur laquelle l'outil fonctionne.
C'est aussi une meilleure séparation des responsabilités.

## Structure
- `Popote.Data/` (lib `net10.0`) : `Models/`, `Data/AppDbContext.cs`,
  `Data/AppDbContextFactory.cs` (fabrique design-time), `Migrations/`.
  Packages : `Microsoft.EntityFrameworkCore.Sqlite` + `.Design`.
- `Popote.csproj` (app MAUI) référence `Popote.Data` et **exclut** son dossier de sa
  propre compilation (`<Compile Remove="Popote.Data\**\*.cs" />`).
- Les XAML qui référencent une entité utilisent `clr-namespace:Popote.Models;assembly=Popote.Data`.

## Démarrage
`MauiProgram` appelle `db.Database.Migrate()` au lancement : crée la base au premier
lancement et applique les migrations en attente ensuite.

## Ajouter une migration (à refaire à chaque changement de modèle)
```sh
cd Popote.Data
dotnet ef migrations add <NomExplicite>
```
La migration est appliquée automatiquement au prochain démarrage de l'app.

## Choix techniques
- Fabrique **design-time** (`AppDbContextFactory`) : donne une connexion SQLite locale
  à l'outil (l'app, elle, configure la vraie base via DI dans `MauiProgram`).

## Reste à faire / limites connues
- Transition depuis l'ancien `EnsureCreated()` : une base déjà créée n'a pas d'historique
  de migration → il faut **vider les données de l'app une fois** (`adb shell pm clear onl.nci.popote`
  ou réinstaller) pour que la 1re migration parte sur une base vierge.
