# Refonte « Mise en place » — Moments (1.2.0)

> Statut : en cours — 22/10/2026 (branche `refonte/moments`)

## Objectif
Donner vie aux instants clés de la cuisine (planche, section 07) sans gêner la saisie ni la
lecture : chaque mouvement est déclenché par un geste, interruptible, et suit le niveau de
mouvement (`MotionTokens.Level`).

## Comportement attendu
Ordre de réalisation (du plus simple au plus risqué) :
- [x] *Article coché* : le bol se remplit au geste (courbe Release, 560 ms), vibration `Tick`.
- [x] *Recette enregistrée* : le bouton « Enregistrer » se change en pastille cochée,
  vibration `Confirm`, pause de 0,4 s, puis retour.
- [ ] *Étape suivante* (mode cuisson).
- [ ] *Minuteur fini*.
- [ ] *Passage au poste*.
- [ ] *La mise en place* (ouverture d'une recette).
- [ ] *Transitions de page*.

Niveaux : Full = mouvement complet ; Reduced (Sobre) = fondu ou remplissage linéaire de
150 ms ; None (« Supprimer les animations ») = état final direct, sans pause.

## Fichiers concernés
- `Controls/Bowl.cs` — `FillAsync` (remplissage selon le niveau) et `StopFill`.
- `Controls/BowlCheck.cs` — remplit au geste seulement ; `Update` coupe tout remplissage en cours.
- `ViewModels/RecipeEditViewModel.cs` — `IsSaved`, `Confirm`, pause avant le retour.
- `Views/RecipeEditPage.xaml(.cs)` — pastille cochée (`SavedBadge`, `RiseInAsync`).
- `Resources/Strings/AppResources.resx` — `Edit_Saved`.

## Choix techniques
- Le remplissage n'est lancé que par `Activate` (le geste), jamais par le changement de
  `IsChecked` : une cellule recyclée par la `CollectionView` ne rejoue pas l'animation.
- Le bouton du bas est remplacé par une pastille plutôt que désactivé : pendant la commande
  asynchrone, le style « Disabled » (opacité 0,4) aurait été visible pendant la pause.
- Libellé barré d'un coup (pas de trait animé) : un trait dessiné ne suit pas un libellé
  sur deux lignes (police 200 %).

## Modèle de données impacté
Aucun.

## Reste à faire / limites connues
- Mesures téléphone avant / après (Expressive puis Sobre) : à faire au retour du téléphone.
- Enregistrer depuis la coche de l'en-tête : la pastille du bas peut être hors écran ; la
  vibration et la pause restent.
