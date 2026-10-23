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
- [x] **Import par IA (1.4.0)** : consignes copiées pour une IA, réponse collée dans le formulaire (aperçu + points à vérifier), partage au même format, analyseur tolérant testé. _[DATA + DEV]_ — voir `docs/ai-import.md`.

## Reprise (prochaine session)
> À lire en premier dans une conversation neuve. État au 23/10/2026.

**Où on en est**
- `main` = **1.4.0** (code 4), historique linéaire, poussé. Releases GitHub v1.2.0, v1.3.0 et
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
1. **Vitrine** : refaire le README (jalon ci-dessous), captures sur l'émulateur avec le jeu de démo.

## En cours
- [ ] **Vitrine — refaire le README** _[DESIGN]_ — **en dernier**, quand le reste de la roadmap est
  fini (les captures doivent montrer l'app finale). Même format que les autres apps de l'auteur,
  modèle propre : [dromos](https://github.com/SlashXIV/dromos) (voir aussi
  [kalos-app](https://github.com/SlashXIV/kalos-app), dont seul l'en-tête suit le format) :
  - en-tête centré : icône 112 px (`design/icon/popote-icon.svg`), titre `<h1>`, accroche en gras
    + une ligne de traits (« 100 % hors ligne · sans compte ni cloud · … ») ;
  - 4 captures WebP côte à côte (`width="200"`, `alt` descriptif en français) dans
    `design/screenshots/` : liste, détail, mode cuisson, courses — jeu de démo, barres système retirées ;
  - `---`, un paragraphe d'intro, puis Fonctionnalités (groupées), Installer (APK de la dernière
    Release, mise à jour par-dessus sans perte), Vie privée, Compiler, Crédits (polices OFL), Licence.

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

## Plus tard / idées
- [ ] Import de mes notes existantes (après l'import par IA, qui couvre le texte collé).
- [ ] Réduire le coût CPU du minuteur (≈ 1,1 s par minute de décompte) : le libellé relance la
  mise en page de la carte à chaque seconde.
- [ ] Miniatures des photos à l'import si la liste réelle grossit (PSS ≈ 346 Mo avec des photos 4000 × 3000).
- [ ] Habiller les dialogues natifs restants (alertes, sélecteur, feuille d'actions de la Semaine).
- [ ] Corriger l'alerte NU1903 (`SQLitePCLRaw` transitif) quand une version corrigée sera tirable par EF Core.

## Non-objectifs (pour ne pas se disperser)
- Pas de backend / cloud / compte utilisateur pour l'instant : 100 % local.
- Pas de support iOS tant que la version Android n'est pas stable.
