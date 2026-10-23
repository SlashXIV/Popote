# ROADMAP — Popote

> **Objectif (north star)** : une app Android locale, simple et fiable, pour recenser mes recettes,
> qui me fait progresser sur .NET à parts égales côté données et côté logiciel.
> Toute décision technique sert ce but. En cas de doute : on simplifie.

## Fait
- [x] Squelette : CRUD recettes (titre, portions, préparation), EF Core/SQLite, navigation Shell, MVVM.
- [x] Coquille de projet MAUI compilable (`.csproj`, `App`, `Platforms/Android`, `Resources/`) ; build `net10.0-android` vert. _[DEV]_ — voir `docs/project-setup.md`.
- [x] Édition des ingrédients d'une recette (lignes ingrédient + quantité + unité). _[DATA]_ — voir `docs/recipe-ingredients.md`.
- [x] Page « liste de courses » : sélection multi-recettes → agrégation par rayon. _[DATA]_ — voir `docs/shopping-list.md`.
- [x] Identité : nom **Popote**, logo (monogramme « P » olive + barre or) et design system (base neutre chaude, accent teal). _[DESIGN]_ — voir `docs/design-system.md`.
- [x] Page détail + portions ajustables (chips d'ingrédients, étapes numérotées, recalcul via `ServingsScaler`). _[DEV + DATA]_ — voir `docs/recipe-detail.md`.
- [x] Aide à la saisie d'ingrédient : unité en liste déroulante + suggestions cliquables du catalogue (sans dépendance, plutôt qu'UraniumUI). _[DEV + DATA]_
- [x] Édition du rayon des ingrédients (Picker par ligne) → pastilles colorées du détail + meilleur regroupement des courses. _[DATA]_
- [x] Liste de courses actionnable : cases à cocher pour barrer les articles. _[DEV]_
- [x] Photo du plat (galerie/appareil photo via `MediaPicker`) : miniature en liste, photo en détail. _[DEV]_ — voir `docs/recipe-photo.md`.
- [x] Tags et filtres (puces à bascule + filtre ET). _[DATA]_ — voir `docs/recipe-tags.md`.
- [x] Suppression d'une recette depuis l'UI (balayage + confirmation). _[DEV]_
- [x] Temps de prépa / cuisson : édition + affichage au détail. _[DEV + DATA]_
- [x] Migrations EF Core (couche données `Popote.Data`, remplace `EnsureCreated`). _[DATA]_ — voir `docs/data-migrations.md`.
- [x] Planificateur de repas (semaine → liste de courses). _[DATA + DEV]_ — voir `docs/meal-planner.md`.
- [x] Tests unitaires (ServingsScaler + agrégation des courses, xUnit). _[DEV]_ — voir `docs/tests.md`.
- [x] Nettoyage des API MAUI dépréciées (`DisplayAlertAsync`, `PickPhotosAsync`…). _[DEV]_
- [x] Recherche par ingrédients (« cuisiner avec X, Y », cumul ET). _[DATA]_ — voir `docs/cook-with.md`.
- [x] Mode cuisson : étapes défilables, écran maintenu allumé, minuteur. _[DEV]_ — voir `docs/cooking-mode.md`.
- [x] Favoris (★) + notes perso par recette. _[DATA + DEV]_ — voir `docs/favorites-notes.md`.
- [x] Build Release Android **signé** (APK autonome ; trimming off pour EF Core). _[DEV]_ — voir `README.md`.
- [x] Publication GitHub + **Release** (APK via lien `latest`). _[DEV]_
- [x] Sauvegarde / restauration de la base (`.db3` partageable + import). _[DATA + DEV]_ — voir `docs/backup-restore.md`.
- [x] Liste de courses **persistante** : article manuel + cases cochées persistées + retrait/vidage. _[DATA]_ — voir `docs/shopping-list.md`.
- [x] Partage d'une recette en texte (feuille de partage système). _[DEV]_
- [x] Tri de la liste des recettes (Favoris / Récent / Titre / Temps). _[DATA]_
- [x] **Refonte « Mise en place » — Fondations (1.2.0)** : métaphore du bol, base safran/aubergine, thème sombre, Réglages, typo, jetons, contrôles maison, textes externalisés, logo. _[DESIGN + DEV]_ — voir `docs/redesign-foundations.md` et `docs/design-system.md`.
- [x] **Refonte — Moments (1.3.0)** : 7 moments (article coché, recette enregistrée, étape suivante, minuteur fini, passage au poste, mise en place, transitions de page), mesurés sur téléphone sans régression. _[DESIGN + DEV]_ — voir `docs/redesign-moments.md`.
- [x] **Vitrine (1.4.1)** : README sur le modèle de dromos (icône, 4 captures WebP du jeu de démo, fonctionnalités, installer, vie privée, compiler, crédits), licence AGPL-3.0, permission Internet retirée. _[DESIGN]_ — voir `README.md`, `docs/compiler.md`.
- [x] **Fenêtres natives habillées (1.4.1)** : alertes, feuille d'actions et listes aux couleurs, coins et polices de l'app, clair et sombre ; balayage « Supprimer » lisible. _[DESIGN]_ — voir `docs/design-system.md`.
- [x] **Import par IA (1.4.0)** : consignes copiées pour une IA, réponse collée dans le formulaire (aperçu + points à vérifier), partage au même format, analyseur tolérant testé. _[DATA + DEV]_ — voir `docs/ai-import.md`.

## Reprise (prochaine session)
> À lire en premier dans une conversation neuve. État au 23/10/2026.

**Où on en est**
- `main` = **1.4.1** (code 5), historique linéaire, poussé. Releases GitHub v1.2.0, v1.3.0 et
  **v1.4.0** (dernière) publiées le 23/10/2026, toutes signées avec la nouvelle clé.
- Le téléphone de l'auteur a la **1.4.0 signée** (nouvelle clé) et ses données restaurées.
- Reste de la roadmap : la **Vitrine** (README), puis les idées « Plus tard ».
- Lire d'abord `docs/design-system.md`, `docs/redesign-foundations.md`, `docs/redesign-moments.md`,
  `docs/ai-import.md`. Planche validée : https://claude.ai/code/artifact/f73a7898-93b7-4c17-bea9-41e47c58afe5

**Livrer une version (procédure qui marche)**
1. `ApplicationDisplayVersion` / `ApplicationVersion` (+1) dans `Popote.csproj`, section datée du CHANGELOG.
2. **Dossier propre** (`bin`/`obj` supprimés) : un build incrémental mêlé à un build Debug a produit
   un APK qui plantait au démarrage (`UnsatisfiedLinkError` sur `MauiApplication.n_onCreate`).
3. `dotnet publish -c Release -f net10.0-android -p:AndroidPackageFormat=apk -p:AndroidSigningKeyStore=<chemin>\popote.keystore
   -p:AndroidSigningKeyPass=file:<fichier> -p:AndroidSigningStorePass=file:<fichier>` — le fichier contient
   le mot de passe **sur deux lignes** (apksigner lit une ligne par mot de passe). Clé et mot de passe :
   Bitwarden de l'auteur ; copie locale hors dépôt dans `%USERPROFILE%\popote-sauvegardes\`.
4. `apksigner verify --print-certs` (empreinte SHA-256 du certificat `0a4b62ae…1d11`), installation
   `adb install -r` sur le téléphone (accepter l'invite du Xiaomi), démarrage vérifié, puis tag, push et
   Release (`Popote.apk`, accord de l'auteur).

**Environnement (poste de l'auteur)**
- `global.json` **local, hors dépôt** (`.git/info/exclude`) : épingle le SDK **10.0.303** — la charge
  MAUI n'existe que pour la bande 10.0.300 (un SDK 10.0.402 est aussi installé, sans MAUI).
- **Release sans AOT** (`RunAOTCompilation=false`) : l'AOT exige le trimming, exclu pour EF Core ;
  AOT sans trimming = plantage au démarrage (assertion Mono `class-init.c:2474`).
- Outils **locaux** dans `.claude/outils/` (voir `LISEZMOI.md`) : `ui.py`, `rec.sh` (vidéo découpée à
  20 i/s), `bench.sh`, jeu de démo (`seed/`, `mkphotos.py`, `push-demo.sh`), `str.py` (resx).
- Appareils : **émulateur** `emulator-5554` pour coder et capturer ; **téléphone** (Xiaomi 2506BPN68G,
  Android 17) pour les mesures et la vérification des Release. Mesures : copie `onl.nci.popote.perf`
  (Debug pour pousser les données, puis Release par `adb install -r`, **jamais** par `-t:Install`, qui
  désinstalle et vide la base).

**Prochaines étapes**
- La roadmap est terminée (Vitrine faite en 1.4.1). Suite : « Prochain lot » ci-dessous,
  en refaisant les captures du README (`design/screenshots/`, jeu de démo, thème clair, barres
  système retirées) si l'interface change.

## En cours
- Rien : voir « Prochain lot ».

### Refonte — preuves
| Jalon | Version | Preuve |
|---|---|---|
| Moments | 1.3.0 | téléphone (Release, avant / après, Expressive et Sobre) : démarrage ≈ 1,76 s inchangé, liste p90 7–8 ms, carrousel p90 5 ms ; minuteur 0,95 → 1,03–1,13 s CPU/min ; émulateur : vidéo 20 i/s, animations coupées |
| Fondations | 1.2.0 | 50 tests verts (dont contrastes/daltonisme des 2 thèmes, aucun texte en dur) ; build sans avertissement nouveau ; émulateur : 8 pages en clair et en sombre, police 200 %, densité 600, arbre TalkBack ; téléphone (Release) : démarrage 1,76 s, liste p90 7 ms, carrousel p90 5 ms |

### Journal des décisions (refonte)
| Décision | Raison |
|---|---|
| Métaphore unique : la mise en place (le bol) | la cohérence d'une métaphore, pas la quantité d'effets, fait l'identité |
| Planche validée avant le code | on juge l'ensemble avant de coder écran par écran |
| Base safran + aubergine (pas blanc/vert) | demande explicite : plus « pétillant » |
| Émaux de rayon calculés et testés | donnée codée par la couleur : 3:1 + distincts en protan/deutan |
| Aucune dépendance d'interface | MAUI + contrôles maison suffisent ; pas de lock-in |
| Barre d'onglets Shell gardée, rendu maison | navigation, retour et TalkBack natifs gratuits |
| `Bowl` / `Icon` en GraphicsView | mesuré : les formes MAUI empilées coûtaient des images au défilement |
| Release sans AOT | AOT sans trimming = plantage au démarrage ; trimming exclu pour EF Core |
| Mesures sur téléphone uniquement | l'émulateur varie trop d'un passage à l'autre pour comparer |

## Prochain lot (journée du 23/10/2026)
> Ordre conseillé : du plus sûr au plus long. Chaque sujet suit le workflow du projet (plan court,
> doc, CHANGELOG, build + tests réellement lancés, émulateur), puis une version de correctif 1.4.x
> ou 1.5.0 publiée sur accord. 📱 = le téléphone est nécessaire pour la mesure finale (débranché
> aujourd'hui : coder et vérifier sur l'émulateur, mesurer au retour du téléphone).

### 1. Corriger l'alerte NU1903 _[DATA]_ — environ 15 min
- **Constat** : `SQLitePCLRaw.lib.e_sqlite3` 2.1.11, tiré par EF Core 10.0.9, porte une faille connue
  (GHSA-2m69-gcr7-jv3q, versions ≤ 2.1.11). **EF Core 10.0.13 tire SQLitePCLRaw 2.1.13** (corrigé).
- **Approche** : passer `Microsoft.EntityFrameworkCore.Sqlite` / `.Design` de 10.0.9 à **10.0.13** dans
  `Popote.csproj` et `Popote.Data/Popote.Data.csproj` (et `Microsoft.Extensions.Logging.Debug`, même
  train de versions). Pas de nouvelle dépendance ; aucune migration attendue.
- **Vérifier** : `dotnet list package --vulnerable --include-transitive` vide, build sans NU1903,
  tests verts, démarrage et enregistrement d'une recette sur émulateur (les migrations se rejouent).

### 2. Sauvegarde complète, photos comprises, et restauration sûre _[DATA + DEV]_ — ½ journée
- **Constat** (migration du téléphone, 23/10) : la sauvegarde ne contient que `recettes.db3` ; les
  photos ont dû être rattachées à la main. Et « Restaurer » **écrase la base sans vérifier le fichier
  ni garder de copie** : un mauvais fichier rendrait l'app inutilisable.
- **Approche** :
  - Sauvegarde = une archive `popote-sauvegarde-AAAAMMJJ-HHmm.zip` (`System.IO.Compression`, déjà dans
    .NET) : `recettes.db3` (après `CheckpointAsync`) + les photos référencées (`recipe_*.jpg|png`).
  - Restauration : accepter le `.zip` **et** l'ancien `.db3` (compatibilité). Avant d'écraser : vérifier
    l'en-tête SQLite (`SQLite format 3\0`) et la présence de la table `Recipes`, copier la base actuelle
    en `recettes.avant-restauration.db3`, puis extraire ; en cas d'échec, remettre la copie.
  - Photos : chemins réécrits vers le dossier de l'app à la restauration (`PhotoPath` est un chemin
    absolu ; le nom de fichier suffit à le reconstruire).
  - Logique pure dans `Popote.Data` (ex. `BackupArchive` : liste des fichiers, validation de l'en-tête,
    réécriture des chemins), testée ; l'UI reste dans `SettingsViewModel`.
- **Tests** : archive aller-retour (base + photos), fichier non SQLite refusé, ancienne sauvegarde
  `.db3` acceptée, chemins de photos réécrits.
- **Vérifier** : émulateur, jeu de démo → sauvegarder, désinstaller, réinstaller, restaurer : recettes
  **et photos** revenues. Doc `docs/backup-restore.md` (la limite « photos non incluses » disparaît).

### 3. Importer plusieurs recettes d'un coup (mes notes existantes) _[DATA + DEV]_ — ½ journée
- **Constat** : l'import par IA (1.4.0) traite une recette à la fois ; reprendre un carnet de notes
  serait fastidieux.
- **Question à trancher d'abord** : où sont les notes (Google Keep, Samsung Notes, fichiers texte,
  captures…) et combien ? La réponse fixe la source (coller du texte suffit-il ?).
- **Approche** (si texte collé) : `RecipeTextFormat.ParseMany` découpe sur les lignes `# Titre`
  (même format, mêmes avertissements par recette) ; consignes IA « plusieurs recettes » (variante du
  resx `Import_Instructions`) ; écran « Importer plusieurs recettes » (Réglages) : coller → liste des
  recettes reconnues avec leurs avertissements, cases à cocher → enregistrer la sélection via
  `SaveRecipeAsync`. Doublons de titre signalés, jamais écrasés.
- **Tests** : découpage (0, 1, n recettes ; texte avant la première), avertissements rattachés à la
  bonne recette, aller-retour avec `Write`.

### 4. Miniatures des photos _[DEV]_ — ½ journée — 📱 mesure
- **Constat** : la liste (`RecipeListPage.xaml`) et le détail affichent la photo d'origine (jusqu'à
  4000 × 3000) ; avec le jeu de démo, la mémoire de la liste passe d'environ 295 Mo sans photos à
  365–371 Mo avec (`bench.sh`, PSS).
- **Approche** : à la copie de la photo (`RecipeEditViewModel.CopyToAppDataAsync`), écrire aussi une
  miniature `<nom>.min.jpg` (≈ 360 px de large, JPEG 80) avec `Microsoft.Maui.Graphics`
  (`PlatformImage.FromStream` + `Downsize`) ; la liste affiche la miniature si elle existe, sinon
  l'original (convertisseur, aucun changement de schéma). Photos existantes : miniatures créées en
  arrière-plan au démarrage si absentes. Le détail garde l'original (photo plein écran).
- **Vérifier** : émulateur (rendu identique, pas de flou visible) ; 📱 `bench.sh` avant / après,
  objectif PSS liste nettement sous 300 Mo, défilement p90 inchangé.

### 5. Réduire le coût CPU du minuteur _[DEV]_ — 2 h — 📱 mesure
- **Constat** : décompte = 0,95 s CPU par minute avant les Moments, 1,03–1,13 s après. Chaque
  seconde, le texte du `NumberLabel` (style `Timer`, 48 pt) change dans une grille à colonnes `Auto`,
  dans une carte, dans une page à lignes `Auto` : la mise en page remonte probablement jusqu'à la page
  (carrousel compris).
- **Approche** : mesurer d'abord (une minute de décompte, `bench.sh`), puis isoler la mise en page :
  taille fixe pour le libellé (largeur de « 00:00 » en chiffres tabulaires, hauteur fixée), colonne
  fixe dans la carte. Ne rien changer au `DispatcherTimer` (1 Hz suffit). Garder l'annonce TalkBack
  unique de fin.
- **Vérifier** : 📱 CPU par minute avant / après (objectif ≤ 0,6 s), aucun saut visuel du décompte.

## Plus tard / idées
- [ ] Bouton « Supprimer » en rouge dans les confirmations : impossible avec les fenêtres natives
  habillées ; seulement si un dialogue maison devient utile ailleurs.
- [ ] Jeu de démo plus varié (supprimer les doublons « … de mamie ») avant de refaire les captures du README.

## Non-objectifs (pour ne pas se disperser)
- Pas de backend / cloud / compte utilisateur pour l'instant : 100 % local.
- Pas de support iOS tant que la version Android n'est pas stable.
