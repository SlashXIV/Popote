# Changelog

Toutes les modifications notables du projet sont consignées ici.
Format inspiré de [Keep a Changelog](https://keepachangelog.com/fr/1.1.0/),
versionnage [SemVer](https://semver.org/lang/fr/).

## [Unreleased]

## [1.3.0] — 2026-10-23 — Refonte « Mise en place » : moments

### Added
- *Article coché*, le bol se remplit au geste (rebond, fondu court
  en Sobre, plein d'emblée sans animations) ; *recette enregistrée*, le bouton se change en
  coche avec une vibration de confirmation avant le retour.
- Moments du mode cuisson : *étape suivante*, le bol de l'étape faite se remplit (le
  carrousel ne glisse plus quand les animations sont coupées) ; *minuteur fini*, un
  bol-minuteur (vide, posé pendant le décompte, plein à la fin) déborde trois fois.
- Moments d'ouverture : *la mise en place* (photo qui se pose, titre qui monte, ingrédients en
  cascade, une seule fois), *passage au poste* (bols de progression en cascade, vibration) et
  **transitions de page** maison (fondu + montée) à la place du glissement natif ; retour
  arrière instantané.
- **Import par IA** : dans le formulaire, « Copier les consignes » (format, unités, rayons et
  tags autorisés ; la recette en cours pour la remodeler) puis « Coller la réponse » remplit le
  formulaire, avec les points à vérifier. Analyse tolérante (synonymes, fractions, réponse
  bavarde d'une IA).

### Changed
- « Partager » écrit le format texte de Popote (réimportable tel quel) ; la signature
  « partagé depuis Popote » est retirée, elle aurait fini dans les notes au réimport.

### Performance
- Mesuré sur téléphone (Release) : démarrage, liste et carrousel inchangés ; le minuteur coûte
  ≈ 0,1 s de CPU de plus par minute (voir `docs/redesign-moments.md`).

## [1.2.0] — 2026-10-22 — Refonte « Mise en place » : fondations

> Numérotée 1.2.0 : la Release GitHub « v1.1 » (juillet) a déjà livré les fonctions d'avant la
> refonte, l'app y affichant encore « 1.0 ».

### Added
- **Identité « Mise en place »** : le bol vu de dessus comme unité visuelle (ingrédients,
  courses, progression du mode cuisson, repas de la semaine) ; base crème, safran pour agir,
  aubergine pour la sélection ; émaux de céramique pour les rayons.
- **Thème sombre** réellement utilisable, **Système** par défaut, réglable.
- **Page Réglages** : apparence, Ambiance (Expressive / Sobre), vibrations, sauvegarde et
  restauration (déplacées depuis le menu ⋮).
- **Nouveau logo** (« P » à panse-bol, filet safran), icône monochrome Android 13+, splash.
- Polices **Bricolage Grotesque** + Inter, **chiffres tabulaires** pour quantités et minuteur.
- Contrôles maison accessibles : en-tête, boutons icônes, champs, stepper de portions,
  case-bol, chips, sélecteur segmenté, interrupteur.
- Système de mouvement centralisé (jetons, courbes, niveaux Full / Reduced / None, réglage
  Android « Supprimer les animations » respecté) et haptique désactivable.
- Tests : contrastes WCAG + daltonisme (protan / deutan) des deux thèmes, aucun texte en dur,
  cohérence du resx et typographie française, courbes et niveaux de mouvement, format des quantités.

### Changed
- Toutes les pages redessinées (liste, détail, mode cuisson, édition, Cuisiner avec, semaine, courses).
- Textes de l'interface externalisés dans `Resources/Strings/AppResources.resx`.
- Quantités accordées (« 4 pièces »), décimales et dates en français, espace insécable avant l'unité.
- Libellés TalkBack complets (« Farine, 250 grammes, rayon Épicerie »), titres de niveau 1 et 2.
- Release compilé sans AOT (voir `docs/redesign-foundations.md`).

### Fixed
- Jours de la semaine affichés en anglais selon la langue du téléphone.
- Chips tronqués dans « Cuisiner avec… » (« Carott »).
- Enregistrer une recette sans titre ne faisait rien : un message explique quoi corriger.
- Les vibrations faisaient planter l'app (permission `VIBRATE` manquante).
- Réenregistrer une recette **qui a des tags** faisait planter l'app (liaison recette-tag
  suivie deux fois par EF Core). Présent depuis l'ajout des tags.
- Rouvrir l'app après l'avoir quittée par « retour » la faisait planter (fenêtre MAUI recréée
  dans le même processus) : « retour » sur une page racine la met désormais en arrière-plan.
- Enregistrer une recette dont un ingrédient commence par une lettre accentuée ou « Œ »
  (« Œufs », « Échalote ») plantait, et ce type de tag (« Été ») était dupliqué : la recherche
  insensible à la casse passait par `lower()` de SQLite, limité à l'ASCII.
- Après « Modifier », la page de la recette montrait encore l'ancienne version (titre, photo…)
  jusqu'à la rouvrir : elle se relit désormais au retour, en gardant les portions choisies.

### Removed
- Restes du modèle MAUI (OpenSans, `dotnet_bot.png`, `Colors.xaml`, `Styles.xaml`),
  `AisleToColorConverter` (couleurs en dur).

### Added
- Squelette initial : CRUD recettes (titre, portions, préparation).
- Couche données EF Core / SQLite : entités `Recipe`, `Ingredient`, `RecipeIngredient`, `Tag`, `RecipeTag`.
- `RecipeService` : CRUD + génération de liste de courses agrégée (`GroupBy` + `Sum`).
- `ServingsScaler` : mise à l'échelle des portions.
- Navigation Shell, injection de dépendances et création de la base au démarrage (`MauiProgram`).
- **Coquille de projet MAUI compilable** : `Popote.csproj` (cible `net10.0-android`),
  classe `App`, `Platforms/Android`, `Resources/` (polices, icône, splash, styles).
  Le projet compile désormais (`dotnet build -f net10.0-android` : 0 erreur).
- README racine : prérequis et procédure de build documentés.

- Édition des ingrédients d'une recette : lignes `nom + quantité + unité`,
  ajout/suppression, persistées via `RecipeService`. Catalogue d'ingrédients
  trouver-ou-créer insensible à la casse.
- Liste de courses : onglet « Courses », sélection multi-recettes → liste agrégée
  regroupée par rayon (réutilise `BuildShoppingListAsync`). Navigation par onglets (`TabBar`).
- Page détail (consultation) avec **portions ajustables** : recalcul des quantités
  via `ServingsScaler`, ingrédients en chips (pastille de rayon), préparation en étapes numérotées.
- Détail : **multiplicateur de portions** (×½ ×1 ×2 ×3) relatif à la recette de base, plus intuitif.
- Édition : **unité d'ingrédient via liste déroulante** (Picker) au lieu de texte libre (évite les doublons d'unités).
- Édition : **suggestions cliquables du catalogue** (« Déjà utilisés ») pour ajouter vite un ingrédient connu et favoriser la réutilisation.
- Édition : **rayon de l'ingrédient** (Picker par ligne) — colore les pastilles du détail et affine le regroupement de la liste de courses. Le rayon est mémorisé sur l'ingrédient du catalogue.
- Liste de courses : **cases à cocher** pour barrer les articles au fur et à mesure des courses (état éphémère, réinitialisé à chaque génération).
- **Photo du plat** : ajout via galerie ou appareil photo (`MediaPicker`), copiée en stockage privé ; miniature en liste, photo en tête du détail.
- **Tags & filtres** : tags à bascule sur une recette (+ création), affichés au détail, et filtre de la liste par tags (cumul ET).
- **Suppression d'une recette** depuis la liste : balayage vers la gauche + confirmation.
- **Temps de préparation / cuisson** : éditables sur la recette, affichés au détail (« Prépa 15 min · Cuisson 20 min »).
- **Planificateur de repas** : onglet « Semaine », recettes par jour (navigation entre semaines), et génération de la liste de courses de la semaine.
- **Recherche par ingrédients** (« Cuisiner avec… ») : cocher des ingrédients → recettes qui les contiennent tous.
- **Mode cuisson** : étapes défilables (`CarouselView`), écran maintenu allumé, minuteur avec décompte et alerte.
- **Favoris & notes** : étoile (bascule au détail), notes perso par recette, favoris triés en tête de liste.
- **Build Release signé** : configuration de signature APK (keystore hors dépôt), trimming désactivé pour EF Core ; APK autonome installable sans PC.
- **Sauvegarde / restauration** des données : export du `.db3` (partageable) et import depuis un fichier, via le menu ⋮ des Recettes.
- **Liste de courses persistante** : articles conservés en base, ajout manuel d'articles, cases cochées persistées (survivent au redémarrage), retrait et vidage.
- **Partage d'une recette en texte** : depuis le détail (menu ⋮), compose la recette (ingrédients aux portions affichées, étapes) et ouvre la feuille de partage.
- **Tri de la liste des recettes** : Favoris (défaut), Récent, Titre, Temps.

### Tests
- Projet `Popote.Tests` (xUnit) : `ServingsScaler` et l'agrégation des courses
  (`ShoppingListBuilder`, logique pure extraite dans `Popote.Data`).

### Changed
- Code source rangé en dossiers : `Models/`, `Data/`, `Services/`, `ViewModels/`, `Views/`.
- API MAUI dépréciées remplacées par leurs variantes `…Async` (`DisplayAlertAsync`, `PickPhotosAsync`…).
- `RecipeService.SaveRecipeAsync` prend désormais aussi les lignes d'ingrédients et les tags.
- Design system (brief `docs/design-system.md`) : base neutre chaude + accents
  « earthy naturals » (teal principal) en tokens (`AppColors.xaml`), styles de
  composants (`AppStyles.xaml`), police Inter, thème clair forcé. Chrome dé-violetté
  (Shell + barre système + token `Primary` du template + splash/icône).
- Couche données extraite dans une bibliothèque **`Popote.Data`** ; passage aux
  **migrations EF Core** (`Database.Migrate()` au démarrage) à la place d'`EnsureCreated()`.

### Fixed
- Violet résiduel du template MAUI : les tokens `Primary`/`Secondary`/`Tertiary`
  de `Colors.xaml` sont ramenés au teal, donc les contrôles non stylés (boutons ✕,
  « Ajouter un ingrédient »…) ne sont plus violets.
- Chips « Déjà utilisés » tronqués (« Cacao » → « Caca ») : plus de rétrécissement dans le FlexLayout.
- Double numérotation des étapes de préparation (badge + « 1. » déjà tapé) : la numérotation en tête de ligne est retirée.
- Croix de suppression d'ingrédient allégée (✕ gris discret au lieu d'un carré plein).
- Noms d'ingrédients normalisés avec majuscule initiale (« levure » → « Levure »).
- En-tête de rayon coloré (pastille) dans la liste de courses.

### Security
- Avertissement connu NU1903 sur `SQLitePCLRaw.lib.e_sqlite3.android` 2.1.11 (transitif EF Core) — correctif à venir.
