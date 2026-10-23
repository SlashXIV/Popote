# Refonte « Mise en place » — Fondations (1.2.0)

> Statut : livré — 2026-10-22

## Objectif
Sortir Popote de l'allure « contrôles Android par défaut / modèle MAUI » par un système maison
cohérent (métaphore du bol, base safran et aubergine, typo, jetons, contrôles, thème sombre,
textes externalisés), sans toucher aux données ni à la logique métier. Méthode reprise de Dromos :
planche validée avant le code, palettes testées, accessibilité et performance mesurées.

## Comportement attendu
- Toutes les pages passent dans le nouveau langage, en clair **et** en sombre (Système par défaut).
- Nouvelle page **Réglages** (engrenage de l'onglet Recettes) : Apparence, Ambiance
  (Expressive / Sobre), Vibrations, Sauvegarder, Restaurer. Le menu ⋮ disparaît.
- En-têtes maison avec actions en icônes ; onglets avec icônes ; champs sans soulignement.
- Liste : carte avec mise en place miniature (bols des rayons utilisés), méta « 4 portions · 30 min ».
- Détail : photo pleine largeur, barre d'actions sur pastilles, titre « Poster », stepper maison,
  mise en place sur plan de travail, quantités accordées (« 4 pièces »).
- Mode cuisson : poste de travail (grand numéro, bols de progression, gros boutons, minuteur).
- Courses : case-bol qui se remplit, rayons écrits, « 6 sur 8 à prendre », choix des recettes repliable.
- Semaine : dates en français (« lun. 19 »), aujourd'hui en aubergine, repas = bol plein.
- Cuisiner avec : ingrédients rangés par rayon ; plus de chip tronqué.
- Édition : champs maison, message d'erreur si le titre manque, ligne d'ingrédient en carte.
- Nouveau logo (« P » à panse-bol), icône monochrome Android 13+, splash aubergine.

## Fichiers concernés
- `Resources/Styles/AppColors.xaml`, `Tokens.xaml`, `AppStyles.xaml` — couleurs, jetons, styles par usage
  (`Colors.xaml` et `Styles.xaml` du modèle supprimés).
- `Resources/Strings/AppResources.resx` — tous les textes (classe générée par MSBuild, cf. `Popote.csproj`).
- `Resources/Fonts/BricolageGrotesque-*.ttf` — nouvelle police (OpenSans supprimée).
- `Controls/` — `Bowl`, `BowlCheck`, `Icon`, `IconButton`, `Field`, `PageHeader`, `PortionStepper`,
  `ToggleChip`, `ToggleSwitch`, `Segmented`, `NumberLabel`, `A11y`, `IActivatable`, `Tokens`.
- `Motion/` — `MotionTokens`, `MotionLevel`, `CubicBezier`, `Entrances`, `Haptics`.
- `Platforms/Android/Handlers.cs`, `PopoteShellRenderer.cs`, `SystemBars.cs`, `Resources/values(-night)/colors.xml`.
- `Services/SettingsService.cs`, `Services/QuantityFormat.cs` ; `ViewModels/SettingsViewModel.cs`, `Views/SettingsPage.xaml`.
- `Converters/RecipeCardConverter.cs`, `InvertBoolConverter.cs` (`AisleToColorConverter` supprimé).
- Toutes les `Views/*.xaml` et leurs ViewModels (textes, propriétés d'affichage).
- `Services/RecipeService.cs` — la liste charge les ingrédients (rayons des cartes), sans changement de schéma.
- `design/icon/popote-icon.svg`, `Resources/AppIcon/*.svg`, `Resources/Splash/splash.svg`.
- `Popote.Tests/` — `ThemeContrastTests`, `HardcodedUiTextTests`, `ResxTests`, `MotionTests`, `QuantityFormatTests`.

## Choix techniques
- **Aucune dépendance nouvelle** : contrôles en `ContentView`/`GraphicsView` + handlers Android.
- **`Bowl` et `Icon` dessinés (`GraphicsView`)** plutôt qu'en formes MAUI : mesuré sur émulateur,
  les formes empilées faisaient presque doubler les images en retard au défilement.
- **Bols de progression mis à jour en place** (pas de recréation à chaque étape du carrousel).
- **Rôles TalkBack** des contrôles maison par délégué d'accessibilité Android (`A11y`), avec
  activation au double toucher (`IActivatable`).
- **Culture fr-FR forcée** au démarrage : dates et décimales en français quel que soit le téléphone.
- **Barre d'onglets Shell gardée** (navigation, retour, accessibilité natifs) au rendu maison ;
  barre de navigation Shell masquée au profit de `PageHeader`.
- **Release sans AOT** (`RunAOTCompilation=false`) : l'AOT exige le trimming, que l'on garde
  désactivé pour EF Core ; sans trimming, les images AOT font planter l'app au démarrage
  (assertion Mono, constatée sur le téléphone et l'émulateur avec le SDK 10.0.3xx, code
  d'avant refonte compris).
- Icône monochrome : générée par MAUI depuis l'alpha du premier plan ; le creux du bol est
  découpé dans la lettre pour rester lisible.

## Mesures (Release, téléphone Xiaomi 2506BPN68G, Android 17, jeu de démo de 30 recettes / 25 photos)
| Mesure | 1.2.0 |
|---|---|
| Démarrage à froid (médiane de 8) | 1 760 ms |
| Défilement de la liste (3 × 20 gestes) | 3 images en retard sur ≈ 14 000 ; p90 7 ms, p99 11 ms ; PSS ≈ 346 Mo |
| Carrousel du mode cuisson (2 × 10 gestes) | 0 image en retard ; p90 5 ms |
| Minuteur en marche (60 s) | 1,09 s CPU / min |

Pas de comparaison avec la 1.0 sur téléphone : un Release de l'ancien code construit avec la
chaîne d'outils actuelle plante au démarrage (même cause que ci-dessus).

## Modèle de données impacté
Aucun (pas de migration). Les libellés de rayons et d'unités restent des **valeurs de données**.

## Reste à faire / limites connues
- Jalon Moments (1.3.0) : animations des moments, transitions de page.
- Les dialogues natifs (alertes, liste du sélecteur, feuille d'actions de la Semaine) gardent le
  rendu système, seulement recoloré.
- Mémoire de la liste ≈ 346 Mo avec des photos de 4000 × 3000 : miniatures à l'import à étudier
  si la liste réelle grossit.
