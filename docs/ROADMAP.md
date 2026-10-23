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
- [x] **Alerte NU1903** : EF Core 10.0.13 → SQLitePCLRaw 2.1.13 ; aucun paquet vulnérable. _[DATA]_
- [x] **Sauvegarde complète** (`.zip` base + photos) et **restauration sûre** (fichier vérifié, base d'avant gardée et remise en cas d'échec, chemins de photos réécrits, ancien `.db3` accepté). _[DATA + DEV]_ — voir `docs/backup-restore.md`.
- [x] **Importer plusieurs recettes** (texte collé, `ParseMany`, doublons signalés et décochés). _[DATA + DEV]_ — voir `docs/import-many.md`.
- [x] **Miniatures des photos** dans la liste (360 px, orientation EXIF, créées au démarrage si absentes) : défilement p90 12 → 10 ms ; mémoire inchangée (les photos n'étaient pas la cause). _[DEV]_ — voir `docs/recipe-photo.md`.
- [x] **Minuteur moins gourmand** (`TickerLabel` : décompte sans mise en page) : 0,98 → 0,58 s CPU/min sur téléphone. _[DEV]_ — voir `docs/cooking-mode.md`.

## Reprise (prochaine session)
> À lire en premier dans une conversation neuve. État au 23/10/2026 (soir).

**Où on en est**
- `main` = **1.5.0** (code 6), Release GitHub **v1.5.0** publiée le 23/10/2026 (APK signé, empreinte
  `0a4b62ae…1d11`), installée sur le téléphone de l'auteur (données gardées). Lot du 23/10 vérifié
  sur émulateur et téléphone (copie `onl.nci.popote.perf` : banc, sauvegarde `.zip` → vidage →
  restauration, orientation EXIF).
- Mémoire de la liste ≈ 360 Mo : runtime .NET sans AOT + tampons graphiques, pas les photos
  (voir `docs/recipe-photo.md`). Piste éventuelle, non planifiée : rien de simple sans AOT.
- Lire d'abord `docs/design-system.md`, `docs/backup-restore.md`, `docs/import-many.md`.
  Planche validée : https://claude.ai/code/artifact/f73a7898-93b7-4c17-bea9-41e47c58afe5

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

## En cours
- Rien : voir « Plus tard / idées ».

### Refonte — preuves
| Jalon | Version | Preuve |
|---|---|---|
| Lot du 23/10 | 1.5.0 | 100 tests verts, build sans avertissement ; téléphone (Release, 1.4.1 → 1.5.0) : démarrage ≈ 1,8–1,9 s inchangé, liste p90 12 → 10 ms, PSS 359–364 → 362–366 Mo, carrousel p90 5 ms, minuteur 0,98 → 0,58 s CPU/min ; sauvegarde 25 Mo restaurée après vidage |
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
| Minuteur : `TickerLabel` (texte posé côté natif, largeur figée) | mesuré : `Label.Text` relance la mise en page de la page à chaque seconde ; une taille MAUI fixe n'y change rien |
| Sauvegarde `.zip` validée avant d'écraser | un mauvais fichier rendait l'app inutilisable ; `System.IO.Compression` suffit |

## Prochain lot
- Fait le 23/10/2026 (voir « Fait ») ; le détail des approches est dans chaque doc de feature.

## Plus tard / idées
- [ ] Bouton « Supprimer » en rouge dans les confirmations : impossible avec les fenêtres natives
  habillées ; seulement si un dialogue maison devient utile ailleurs.
- [ ] Jeu de démo plus varié (supprimer les doublons « … de mamie ») avant de refaire les captures du README.

## Non-objectifs (pour ne pas se disperser)
- Pas de backend / cloud / compte utilisateur pour l'instant : 100 % local.
- Pas de support iOS tant que la version Android n'est pas stable.
