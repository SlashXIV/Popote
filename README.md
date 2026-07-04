# Popote

App Android locale de gestion de recettes de cuisine (usage personnel).
Stack : **.NET MAUI** (UI/XAML) + **EF Core / SQLite** (données) + **CommunityToolkit.Mvvm** (MVVM).

> Règles de travail → [`docs/CLAUDE.md`](docs/CLAUDE.md) · Objectif et jalons → [`docs/ROADMAP.md`](docs/ROADMAP.md) · Suivi des modifs → [`docs/CHANGELOG.md`](docs/CHANGELOG.md)

## Prérequis
- **SDK .NET 10** (`dotnet --version` ≥ `10.x`).
- **Workload MAUI** : `dotnet workload install maui`.
- **JDK 17** (ex. Microsoft OpenJDK 17) pour la compilation Android.
  ⚠️ Si un JDK plus ancien (8/11) est sur le `PATH`, il faut pointer le build vers le JDK 17
  (voir ci-dessous), ou définir `JAVA_HOME` vers le JDK 17.

## Première mise en place
```sh
# 1. Restaurer les packages
dotnet restore

# 2. Installer les dépendances Android (SDK Android, licences) — une seule fois
dotnet build -t:InstallAndroidDependencies -f net10.0-android `
  -p:JavaSdkDirectory="C:\Program Files\Microsoft\jdk-17.0.19.10-hotspot" `
  -p:AcceptAndroidSDKLicenses=True
```

## Build & run
```sh
# Compiler
dotnet build -f net10.0-android `
  -p:JavaSdkDirectory="C:\Program Files\Microsoft\jdk-17.0.19.10-hotspot"

# Compiler + lancer sur émulateur/appareil connecté
dotnet build -t:Run -f net10.0-android `
  -p:JavaSdkDirectory="C:\Program Files\Microsoft\jdk-17.0.19.10-hotspot"
```
> Astuce : définir `JAVA_HOME` vers le JDK 17 permet d'omettre `-p:JavaSdkDirectory`.

## Build Release (APK signé)
Produit un APK autonome, installable sans PC. Nécessite un **keystore** (clé de
signature) — **non versionné** ; garde-le précieusement, Android exige la **même clé**
pour toutes les mises à jour.

Créer la clé une fois (adapter les mots de passe) :
```sh
keytool -genkeypair -v -keystore popote.keystore -alias popote `
  -keyalg RSA -keysize 2048 -validity 10000 -dname "CN=Popote, O=Popote, C=FR"
```
Publier l'APK signé (mots de passe passés au build, jamais commités) :
```sh
dotnet publish -c Release -f net10.0-android -p:AndroidPackageFormat=apk `
  -p:AndroidSigningKeyPass=<motdepasse> -p:AndroidSigningStorePass=<motdepasse>
```
→ `bin/Release/net10.0-android/publish/onl.nci.popote-Signed.apk`.
Installer : `adb install <chemin>.apk` (désinstaller d'abord une version Debug : signature différente).
> Trimming désactivé en Release (`AndroidLinkMode=None`) car EF Core utilise la réflexion.

## Arborescence
```
Popote.csproj      Projet MAUI (cible net10.0-android)
App.xaml(.cs)           Application ; ouvre AppShell
AppShell.xaml(.cs)      Navigation Shell + routes
MauiProgram.cs          DI, EF Core, création de la base au démarrage
Models/                 Entités (Recipe, Ingredient, RecipeIngredient, Tag, RecipeTag)
Data/                   AppDbContext (EF Core / SQLite)
Services/               RecipeService, ServingsScaler
ViewModels/             Un ViewModel par page (CommunityToolkit.Mvvm)
Views/                  Pages XAML (RecipeListPage, RecipeEditPage)
Platforms/Android/      MainActivity, MainApplication, manifest
Resources/              Polices, icône, splash, styles
docs/                   Documentation (une feature = un fichier)
```

## Documentation
Voir [`docs/README.md`](docs/README.md) pour l'index des features documentées.
