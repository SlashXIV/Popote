# Compiler, signer, publier

> Statut : à jour — 23/10/2026 (déplacé du README racine, devenu la vitrine du projet)

Stack : **.NET MAUI** (UI/XAML) + **EF Core / SQLite** (données) + **CommunityToolkit.Mvvm** (MVVM).
Règles de travail → [`CLAUDE.md`](CLAUDE.md) · Objectif et jalons → [`ROADMAP.md`](ROADMAP.md) ·
Suivi des modifs → [`CHANGELOG.md`](CHANGELOG.md).

## Prérequis
- **SDK .NET 10** de la bande 10.0.300 (la charge MAUI n'existe que pour elle) ; un `global.json`
  local peut l'épingler.
- **Workload MAUI** : `dotnet workload install maui` (puis `dotnet workload update` après une mise
  à jour du SDK).
- **JDK 17** (ex. Microsoft OpenJDK 17). Si un JDK plus ancien est sur le `PATH`, pointer le build
  vers le JDK 17 (`-p:JavaSdkDirectory=…`) ou définir `JAVA_HOME`.

## Première mise en place
```sh
dotnet restore
# Dépendances Android (SDK, licences) — une seule fois
dotnet build -t:InstallAndroidDependencies -f net10.0-android `
  -p:JavaSdkDirectory="C:\Program Files\Microsoft\jdk-17.0.19.10-hotspot" `
  -p:AcceptAndroidSDKLicenses=True
```

## Compiler, tester, lancer
```sh
dotnet build -f net10.0-android -p:JavaSdkDirectory="C:\Program Files\Microsoft\jdk-17.0.19.10-hotspot"
dotnet test Popote.Tests/Popote.Tests.csproj
# Compiler + lancer sur l'émulateur ou l'appareil connecté
dotnet build -t:Run -f net10.0-android -p:JavaSdkDirectory="C:\Program Files\Microsoft\jdk-17.0.19.10-hotspot"
```

## APK Release signé
Android exige la **même clé** pour toutes les mises à jour : `popote.keystore` n'est **jamais
versionné**. La clé et son mot de passe sont rangés hors du dépôt (gestionnaire de mots de passe).

1. Numéro de version : `ApplicationDisplayVersion` et `ApplicationVersion` (+1) dans `Popote.csproj`,
   section datée dans le CHANGELOG.
2. **Partir d'un dossier propre** (supprimer `bin/` et `obj/`) : un build incrémental mêlé à un build
   Debug a déjà produit un APK qui plantait au démarrage (`UnsatisfiedLinkError`).
3. Publier, le mot de passe étant lu dans un fichier hors dépôt qui le contient **sur deux lignes**
   (apksigner lit une ligne pour le keystore, une pour la clé) :
   ```sh
   dotnet publish -c Release -f net10.0-android -p:AndroidPackageFormat=apk `
     -p:AndroidSigningKeyStore=<chemin>\popote.keystore `
     -p:AndroidSigningKeyPass=file:<fichier> -p:AndroidSigningStorePass=file:<fichier>
   ```
   → `bin/Release/net10.0-android/publish/onl.nci.popote-Signed.apk`.
4. Vérifier : `apksigner verify --print-certs <apk>` (empreinte SHA-256 du certificat
   `0a4b62ae…1d11`), puis `adb install -r <apk>` et démarrage sur un vrai téléphone.

Créer une nouvelle clé (seulement si elle est perdue — les installations existantes devront alors
être désinstallées une fois) :
```sh
keytool -genkeypair -keystore popote.keystore -storetype PKCS12 -alias popote `
  -keyalg RSA -keysize 4096 -validity 10000 -dname "CN=Popote, O=Popote, C=FR"
```

> Release **sans trimming** (`AndroidLinkMode=None`, EF Core utilise la réflexion) et donc **sans
> AOT** (`RunAOTCompilation=false`) : l'AOT exige le trimming, et sans lui l'app plante au démarrage.

## Arborescence
```
Popote.csproj           Projet MAUI (cible net10.0-android)
App.xaml(.cs)           Application ; ouvre AppShell
AppShell.xaml(.cs)      Navigation Shell, routes, transitions de page
MauiProgram.cs          DI, EF Core, migrations au démarrage, retour Android
Popote.Data/            Entités, AppDbContext, migrations, logique pure (portions, courses, étapes, format texte)
Popote.Tests/           Tests xUnit (logique + couleurs, textes, mouvement, format d'import)
Services/               RecipeService, SettingsService, QuantityFormat
ViewModels/             Un ViewModel par page (CommunityToolkit.Mvvm)
Views/                  Pages XAML (liste, détail, édition, cuisson, Cuisiner avec, semaine, courses, réglages)
Controls/               Contrôles maison (bol, champ, en-tête, stepper, chips…)
Motion/                 Système de mouvement et haptique
Converters/             Convertisseurs de liaison
Platforms/Android/      MainActivity, handlers, rendu des onglets, barres système, thème des fenêtres natives
Resources/              Polices, icône, splash, styles et jetons, textes (Strings/AppResources.resx)
design/                 Logo commenté (icon/) et captures du README (screenshots/)
docs/                   Documentation (une feature = un fichier)
```
