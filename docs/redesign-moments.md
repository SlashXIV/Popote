# Refonte « Mise en place » — Moments (1.3.0)

> Statut : moments faits, mesurés sur téléphone — 23/10/2026 (branche `refonte/moments`)

## Objectif
Donner vie aux instants clés de la cuisine (planche, section 07) sans gêner la saisie ni la
lecture : chaque mouvement est déclenché par un geste, interruptible, et suit le niveau de
mouvement (`MotionTokens.Level`).

## Comportement attendu
Ordre de réalisation (du plus simple au plus risqué) :
- [x] *Article coché* : le bol se remplit au geste (courbe Release, 560 ms), vibration `Tick`.
- [x] *Recette enregistrée* : le bouton « Enregistrer » se change en pastille cochée,
  vibration `Confirm`, pause de 0,4 s, puis retour.
- [x] *Étape suivante* (mode cuisson) : le bol de progression de l'étape quittée se remplit
  depuis son contenu « posé » (pas de saut) ; carrousel sans défilement animé au niveau None.
- [x] *Minuteur fini* : bol-minuteur dans la carte (vide au repos, posé pendant le décompte,
  plein à la fin) qui se remplit puis déborde trois fois (×1,25), `Confirm`, annonce TalkBack
  unique ; l'alerte « C'est prêt » est gardée (seul signal hors de l'écran).
- [x] *Passage au poste* : la page se pose (transition), les bols de progression arrivent en
  cascade, `Confirm` à la première apparition seulement.
- [x] *La mise en place* : la photo se pose (1,04 → 1 en fondu), le titre monte, les ingrédients
  suivent en cascade. Une seule fois : changer les portions met à jour sur place.
- [x] *Transitions de page* : les 5 pages poussées s'affichent sans glissement natif puis se
  posent (fondu + montée de 12) ; le retour arrière est une coupure nette, sans flash.

Niveaux : Full = mouvement complet ; Reduced (Sobre) = fondu ou remplissage linéaire de
150 ms ; None (« Supprimer les animations ») = état final direct, sans pause.

## Fichiers concernés
- `Controls/Bowl.cs` — `FillAsync` (remplissage selon le niveau) et `StopFill`.
- `Controls/BowlCheck.cs` — remplit au geste seulement ; `Update` coupe tout remplissage en cours.
- `ViewModels/RecipeEditViewModel.cs` — `IsSaved`, `Confirm`, pause avant le retour.
- `Views/RecipeEditPage.xaml(.cs)` — pastille cochée (`SavedBadge`, `RiseInAsync`).
- `Resources/Strings/AppResources.resx` — `Edit_Saved`.
- `Controls/Bowl.cs` — `AnimatesFill` : remplissage rejoué quand l'état passe à Full.
- `Motion/Entrances.cs`, `Motion/MotionTokens.cs` — `OverflowAsync` (3 gonflements ×1,25).
- `ViewModels/CookingModeViewModel.cs` — `TimerBowl`, événement `TimerFinished`.
- `Views/CookingModePage.xaml(.cs)` — bol-minuteur, carrousel selon le niveau de mouvement.
- `AppShell.xaml.cs` — transition d'entrée des routes poussées (`OnNavigating` + `PageAppearing`).
- `Views/*.xaml` (5 pages poussées) — `Shell.PresentationMode="NotAnimated"`.
- `Views/RecipeDetailPage.xaml.cs` — la mise en place ; `Motion/Entrances.cs` — `SettleInAsync`.
- `Resources/Styles/Tokens.xaml` — `OverflowRoom` (marge pour que le bol déborde sans être rogné).

## Choix techniques
- Le remplissage n'est lancé que par `Activate` (le geste), jamais par le changement de
  `IsChecked` : une cellule recyclée par la `CollectionView` ne rejoue pas l'animation.
- Le bouton du bas est remplacé par une pastille plutôt que désactivé : pendant la commande
  asynchrone, le style « Disabled » (opacité 0,4) aurait été visible pendant la pause.
- *Étape suivante* : pas de numéro qui roule. Le numéro vit dans chaque page du carrousel, qui
  glisse déjà ; le faire rouler doublerait le mouvement pendant la lecture.
- `ScaleToAsync` renvoie `true` quand l'animation est **annulée** : la boucle de débordement
  s'arrête alors (page quittée).
- `Shell.PresentationMode` est posé sur chaque page, pas dans le style implicite de page : un
  style implicite s'applique quand la page rejoint l'arbre, après que Shell a choisi l'animation.
- Les briques d'entrée attendent `Loaded` avant d'animer : au moment de `OnAppearing`, les vues
  d'une page poussée ne sont pas encore attachées et MAUI terminait leurs animations aussitôt.
- Titre posé d'un bloc plutôt que lettre à lettre : découper le texte en morceaux animés
  casserait la liaison du titre et le nœud TalkBack unique, pour un effet de 0,4 s.
- Vérification : vidéo de l'émulateur découpée à 20 images/s (`.claude/outils/rec.sh`), et
  ralentie ×10 via l'échelle d'animation Android, que les animations MAUI suivent aussi.
- Libellé barré d'un coup (pas de trait animé) : un trait dessiné ne suit pas un libellé
  sur deux lignes (police 200 %).

## Modèle de données impacté
Aucun.

## Reste à faire / limites connues
- Coût CPU du minuteur : 0,95 → 1,03–1,13 s par minute de décompte (≈ 2 % d'un cœur), non réduit.
  Piste : le libellé du décompte relance la mise en page de la carte à chaque seconde.
- La carte du minuteur change de hauteur à la fin (champ ↔ décompte) : petit saut, existant.
- ~~Mesures téléphone avant / après~~ (faites, voir ci-dessous) (Expressive puis Sobre) : à faire au retour du téléphone.
- Enregistrer depuis la coche de l'en-tête : la pastille du bas peut être hors écran ; la
  vibration et la pause restent.

## Mesures (téléphone Xiaomi 2506BPN68G, Release, jeu de démo de 30 recettes)
Copie `onl.nci.popote.perf`, même jeu de données ; avant = `main` (1.2.0), après = `refonte/moments`.

| Mesure | Avant, Expressive | Avant, Sobre | Après, Expressive | Après, Sobre |
|---|---|---|---|---|
| Démarrage (8 lancements) | 1,74–1,89 s | 1,75–1,80 s | 1,75–1,79 s | 1,77–1,94 s |
| Liste, p90 / p99 | 7 / 10–12 ms | 7–8 / 10–11 ms | 7–8 / 10–12 ms | 7–8 / 12 ms |
| Carrousel, p90 / p99 | 5 / 6 ms | 5 / 6 ms | 5 / 6–7 ms | 5 / 6 ms |
| Minuteur, CPU par minute | 0,96 s | 0,94 s | 1,03 s | 1,13 s |

Aucune régression de la liste ni du carrousel ; démarrage inchangé.
