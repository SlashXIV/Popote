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

## Reprise (prochaine session)
> À lire en premier dans une conversation neuve. État au 22/10/2026.

**Où on en est**
- `refonte/fondations` **fusionnée dans `main`** le 22/10/2026 (ni poussée, ni taguée) ; les Moments
  se développent sur la branche **`refonte/moments`**. `Popote.csproj` est déjà en **1.2.0 (code 2)** ; la Release 1.2.0 n'est pas publiée.
- Lire d'abord `docs/design-system.md` (le système codé) et `docs/redesign-foundations.md`
  (choix, mesures, limites). Planche validée : https://claude.ai/code/artifact/f73a7898-93b7-4c17-bea9-41e47c58afe5
- Rien n'a changé dans `Popote.Data` ni dans le schéma (aucune migration).

**Environnement (poste de l'auteur)**
- `global.json` **local, hors dépôt** (`.git/info/exclude`) : épingle le SDK **10.0.303** — la charge
  MAUI n'existe que pour la bande 10.0.300 (un SDK 10.0.402 est aussi installé, sans MAUI).
  `dotnet workload update` lancé le 22/10 (runtime Mono et compilateur AOT en 10.0.11).
- **Release sans AOT** (`RunAOTCompilation=false`) : l'AOT exige le trimming, exclu pour EF Core ;
  AOT sans trimming = plantage au démarrage (assertion Mono `class-init.c:2474`), y compris pour le
  code d'avant la refonte. Ne pas réactiver l'AOT sans trimming testé avec EF Core.
- Outils de vérification **locaux** dans `.claude/outils/` (voir `LISEZMOI.md`) : pilotage d'appareil
  et captures (`ui.py`, `tour.sh`, `sheet.py`), mesures (`bench.sh`), jeu de démo (`seed/`,
  `mkphotos.py`), calculs de palette, ajout de chaînes au resx (`str.py`), source de la planche.
- Appareils : **émulateur** `emulator-5554` pour captures, TalkBack (`uiautomator dump --compressed`),
  police 200 %, densité ; **téléphone** (Xiaomi 2506BPN68G, Android 17) pour **toutes les mesures** :
  l'émulateur varie trop. Le Xiaomi demande d'accepter chaque installation USB à l'écran.
  Une copie de mesure `onl.nci.popote.perf` (jeu de démo) est installée à côté de l'app réelle ;
  l'app réelle est encore la 1.0 Debug, ses données sont sauvegardées hors dépôt.
- **Depuis le 22/10/2026 : téléphone débranché.** On code et on vérifie sur l'émulateur ; tout ce qui
  exige le téléphone (mesures de performance, démarrage de l'APK Release signé) est **mis de côté**
  et noté « à mesurer au retour du téléphone » dans le jalon concerné, jamais extrapolé de l'émulateur.

**Prochaines étapes, dans l'ordre**
1. **Livrer 1.2.0** : ~~fusionner dans `main`~~ (fait), tag `v1.2.0` sur le commit de fusion,
   APK signé (`dotnet publish -c Release … -p:AndroidSigningKeyPass=… -p:AndroidSigningStorePass=…`,
   mots de passe par l'auteur), vérifier qu'il **démarre sur le téléphone**, Release GitHub. Installer
   par-dessus la 1.0 réelle : même clé ? sinon sauvegarde / restauration des données d'abord.
   *En attente du téléphone* (vérification du démarrage) ; le reste peut avancer sans lui.
2. **Refonte — Moments (1.3.0)**, voir le jalon ci-dessous — sur l'émulateur, mesures au retour du téléphone.
   Faits : 3, 4, 5, 6 (branche `refonte/moments`). Suite : 2, 1, 7 — voir `docs/redesign-moments.md`.
3. **Import par IA (1.4.0)**, voir le jalon ci-dessous.
4. **Vitrine** (en tout dernier, roadmap finie) : refaire le README, voir le jalon ci-dessous.

## En cours
- [ ] **Refonte — Moments (1.3.0)** _[DESIGN + DEV]_ — planche, section 07. Briques déjà prêtes :
  `Motion/MotionTokens` (durées, courbes, `Level`), `Motion/Entrances` (`RiseInAsync`,
  `PressAsync`, `PressScaleBehavior`, encore inutilisés), `Controls/Bowl.FillProgress` (le bol se
  remplit), `Motion/Haptics` (`Tick`, `Confirm`).
  1. *La mise en place* (ouverture d'une recette) : photo 1,04 → 1 en fondu, titre posé lettre à
     lettre (un seul nœud TalkBack), bols d'ingrédients en cascade (50 ms, 8 au plus).
  2. *Passage au poste* (« Cuisiner ») : ouverture du poste en fondu, bols de progression qui
     apparaissent, haptique `Confirm`.
  3. ~~*Étape suivante*~~ (fait) : bol de l'étape faite qui se remplit ; numéro qui roule écarté
     (il est dans la page du carrousel, qui glisse déjà).
  4. ~~*Minuteur fini*~~ (fait) : bol-minuteur qui « déborde » 3 fois puis s'arrête, `Confirm` ; une seule
     annonce TalkBack. **Reste** : coût du minuteur (1,09 s CPU/min), à mesurer au retour du téléphone.
  5. ~~*Article coché*~~ (fait) : le bol se remplit (courbe Release), `Tick` ; libellé barré d'un coup.
  6. ~~*Recette enregistrée*~~ (fait) : le bouton se change en coche (0,4 s), `Confirm`, puis retour.
  7. *Transitions de page* : entrée maison fondu + montée sous contrôle du niveau de mouvement
     (`Shell.PresentationMode=NotAnimated` sur les routes poussées) ; revenir au push natif si flash.
  - Critères : tout est interruptible et déclenché par un événement (jamais de boucle) ; Sobre =
    fondus de 150 ms ; « Supprimer les animations » = état final direct ; aucune gêne à la saisie ;
    rien ne bouge pendant la lecture d'une étape ; mesures téléphone avant / après (même build,
    Ambiance Expressive puis Sobre) sans régression de la liste ni du carrousel ; doc `docs/redesign-moments.md`.
- [ ] **Import par IA (1.4.0)** _[DATA + DEV]_ — préparer, dans l'app, un **copier-coller du format
  attendu** pour qu'une IA externe (ChatGPT, Claude…) crée ou remodèle une recette à importer.
  - Bouton « Copier les consignes pour une IA » (Réglages ou écran d'import) : met dans le
    presse-papiers les consignes + le gabarit + les **listes autorisées** (unités de
    `IngredientLineViewModel.UnitOptions`, rayons de `AisleOptions`, tags existants).
  - L'utilisateur colle ces consignes dans l'IA avec sa recette (texte, photo, lien), récupère le bloc,
    puis « Importer une recette » : coller → **aperçu** (mise en place, étapes) → corriger → Enregistrer.
  - Format proposé (lisible, tolérant, sans JSON à échapper) :
    ```
    # Crêpes de la Chandeleur
    Portions : 4
    Préparation : 10 min
    Cuisson : 20 min
    Tags : Dessert, Rapide
    ## Ingrédients
    - 250 g | Farine | Épicerie
    - 4 pièce | Œufs | Crèmerie
    ## Étapes
    1. Verser la farine dans un saladier…
    ## Notes
    Moins de sucre la prochaine fois.
    ```
  - Analyseur **pur** dans `Popote.Data` (ex. `RecipeTextParser`), testé (champs manquants,
    unités synonymes « gr » → « g », rayon inconnu → « Divers » avec avertissement, quantités
    « 1,5 » / « ½ »), réutilisant `SaveRecipeAsync` (pas de migration). Même format en sortie pour
    « Partager » : aller-retour garanti. Remplace l'idée « Import d'une recette (URL ou texte collé) ».
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
- [ ] Miniatures des photos à l'import si la liste réelle grossit (PSS ≈ 346 Mo avec des photos 4000 × 3000).
- [ ] Habiller les dialogues natifs restants (alertes, sélecteur, feuille d'actions de la Semaine).
- [ ] Corriger l'alerte NU1903 (`SQLitePCLRaw` transitif) quand une version corrigée sera tirable par EF Core.

## Non-objectifs (pour ne pas se disperser)
- Pas de backend / cloud / compte utilisateur pour l'instant : 100 % local.
- Pas de support iOS tant que la version Android n'est pas stable.
