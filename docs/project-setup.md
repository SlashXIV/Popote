# Project setup — coquille MAUI compilable

> Statut : livré — 2026-06-27

## Objectif
Le dépôt ne contenait que le code métier (modèles, services, VM, pages) mais
aucun fichier de projet : pas de `.csproj`, pas de classe `App`, pas de dossier
`Platforms/` ni `Resources/`. Rien ne pouvait compiler. Cette étape pose la
coquille MAUI minimale pour que `dotnet build -f net10.0-android` réussisse.

## Comportement attendu
- `dotnet restore` puis `dotnet build -f net10.0-android` produisent un `.dll`
  sans erreur.
- Le code existant (CRUD recettes) est rangé en dossiers conformément à CLAUDE.md.

## Fichiers concernés
- `Popote.csproj` — projet MAUI ciblant **net10.0-android uniquement**
  (cf. non-objectif « pas d'iOS pour l'instant »). Référence les packages
  `Microsoft.Maui.Controls`, `CommunityToolkit.Mvvm` (8.4.2),
  `Microsoft.EntityFrameworkCore.Sqlite` (10.0.9), `Microsoft.Extensions.Logging.Debug` (10.0.9).
- `App.xaml(.cs)` — classe `Application` ; `CreateWindow` retourne `new Window(new AppShell())`.
- `Platforms/Android/` — `MainActivity`, `MainApplication`, `AndroidManifest.xml`, `colors.xml`.
- `Resources/` — polices OpenSans (réelles), icône, splash, styles, images.
- Réorganisation en dossiers : `Models/`, `Data/`, `Services/`, `ViewModels/`, `Views/`.
- Restent à la racine : `App`, `AppShell`, `MauiProgram.cs`, `Popote.csproj`.

## Choix techniques
- **Coquille générée via `dotnet new maui`** puis fusionnée avec le code existant,
  plutôt qu'écrite à la main (moins d'erreurs sur `Platforms/`, icônes, polices).
- **`MauiProgram.cs` et `AppShell` d'origine conservés** (ils contiennent déjà la
  config EF Core/DI et la route d'édition) — seuls les fichiers manquants ont été ajoutés.
- **Cible mono-plateforme** `net10.0-android` : le template multi-cible (iOS/Mac/Windows)
  a été réduit pour coller à la ROADMAP et éviter des dépendances inutiles.
- **JDK 17 obligatoire** : le `java` du PATH machine est un JDK 8, incompatible avec
  Android .NET 10. On pointe explicitement le build vers le JDK 17 (voir README).

## Modèle de données impacté
Aucun. Étape purement structurelle.

## Reste à faire / limites connues
- ⚠️ Avertissement **NU1903** : `SQLitePCLRaw.lib.e_sqlite3.android 2.1.11`
  (transitif via EF Core) a une vulnérabilité connue. À corriger en épinglant une
  version corrigée de `SQLitePCLRaw.bundle_e_sqlite3` (voir ROADMAP).
- Build **non exécuté sur appareil/émulateur** : seul le `build` a été validé, pas le `-t:Run`.
- Pas encore de tests automatisés (voir ROADMAP).
