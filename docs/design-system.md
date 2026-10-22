# Design system — Popote « Mise en place »

Référence de toute l'interface depuis la 1.1.0. Planche validée le 22/10/2026 (artifact
« Popote, mise en place ») ; ce document décrit ce qui est réellement codé. Objectif : une app
**cohérente, chaleureuse et pétillante**, qui ne fait plus « contrôles Android par défaut ».
La cohérence d'une seule métaphore prime sur la quantité d'effets.

## La métaphore : la mise en place
En cuisine, la mise en place consiste à peser, couper et ranger chaque ingrédient dans son bol
avant d'allumer le feu. Dans Popote, **le bol vu de dessus est l'unité visuelle** : un bord
émaillé, un creux, un contenu (`Controls/Bowl.cs`). Ses trois états racontent l'avancement :

| État | Sens | Où |
|---|---|---|
| vide | à acheter, à venir | liste de courses, étapes à venir, état vide |
| posé | présent, en cours | ingrédient d'une recette, étape en cours |
| plein | coché, fait, planifié | article pris, étape faite, repas posé |

- **Détail** : la liste d'ingrédients est la *mise en place* — un bol par ingrédient sur un
  *plan de travail* (surface `Worktop`, bord supérieur safran).
- **Courses** : la case à cocher est un bol qui se remplit (`Controls/BowlCheck.cs`).
- **Mode cuisson** : le *poste de travail* — un numéro géant, une rangée de bols de progression.
- **Semaine** : un repas posé = un bol plein.
- **Logo** : le « P » dont la panse est un bol (voir `design/icon/popote-icon.svg`).

## Règles d'or
- Pas de blanc pur ni de noir pur ; base crème chaude (clair) ou aubergine très sombre (sombre).
- **Safran** = l'action principale (un bouton plein par écran, au plus) ; **aubergine** = texte
  d'action, sélection, focus. Les émaux de rayon **codent une donnée**, jamais de la déco.
- La couleur n'est jamais la seule information : le libellé du rayon accompagne toujours le bol.
- Aucune couleur, taille, rayon ni espacement en dur dans une page : jetons et styles uniquement.
- Aucun texte d'interface en dur : `Resources/Strings/AppResources.resx` (testé).
- Filets fins (1) plutôt qu'ombres ; une seule ombre douce au plus.
- Le vide fait partie du design ; un écran vide invite à agir (bol vide + phrase + bouton).

## Couleurs — `Resources/Styles/AppColors.xaml`
Chaque rôle existe en paire `<Rôle>Light` / `<Rôle>Dark`. `ThemeContrastTests` relit le fichier.

| Rôle | Clair | Sombre | Usage | Exigence testée |
|---|---|---|---|---|
| Background | `#FBF3E6` | `#1C1620` | fond de page | — |
| Surface | `#FFF9F0` | `#261E2A` | cartes, champs, barre d'onglets | — |
| Worktop | `#F7E6C8` | `#2F2533` | plan de travail, pastilles tonales | — |
| Line | `#EAD8BC` | `#40344A` | filets, bordures | — |
| Ink | `#2A1F2A` | `#F4ECEF` | texte | ≥ 7:1 sur tous les fonds |
| Mute | `#66575F` | `#BCAAB7` | texte secondaire | ≥ 4,5:1 |
| Accent (aubergine) | `#7A2F6B` | `#E3A6D3` | texte d'action, sélection, focus | ≥ 4,5:1 |
| OnAccent | `#FFF9F0` | `#2A1626` | texte sur aubergine | ≥ 4,5:1 |
| AccentSoft | `#F2DCEA` | `#3D2939` | boutons secondaires | Accent ≥ 4,5:1 dessus |
| Highlight (safran) | `#F2A516` | `#F5B53A` | bouton principal, bord du plan de travail | OnHighlight ≥ 4,5:1 |
| OnHighlight | `#2A1F2A` | `#241A10` | texte sur safran | — |
| Favorite | `#A86A00` | `#F5C04A` | étoile | ≥ 3:1 |
| Danger | `#B0303A` | `#FF8E8E` | suppression, erreur | ≥ 4,5:1 |
| Scrim | `#E6FFF9F0` | `#E6261E2A` | pastilles posées sur photo | — |

**Émaux de rayon** (céramique) — donnée codée par la couleur :

| Rayon | Clair | Sombre |
|---|---|---|
| Fruits & légumes (olive) | `#4A8F2A` | `#74A030` |
| Crèmerie (miel) | `#7A5A10` | `#F2D35A` |
| Épicerie (terre cuite) | `#6E3A0C` | `#EFA968` |
| Viande & poisson (sang-de-bœuf) | `#5A1A1E` | `#F27482` |
| Surgelés (cobalt) | `#2477B5` | `#3DAEEF` |
| Divers (ardoise) | `#5C6463` | `#CADBD8` |

Chaque émail tient ≥ 3:1 sur Background, Surface et Worktop, et les six restent distincts deux à
deux pour un œil protanope ou deutéranope (simulation Machado 2009, ΔE OKLab ≥ 8 ; minimum
mesuré : 8,2 en clair, 8,6 en sombre). La correspondance rayon → émail est dans
`AislePalette.RoleFor` (`Controls/Bowl.cs`).

Thème : **Système** par défaut, réglable Clair / Sombre dans Réglages (`SettingsService`,
`Preferences`), appliqué à chaud ; barres système assorties (`Platforms/Android/SystemBars.cs`).

## Typographie — `Resources/Fonts/` (OFL, licences dans `docs/licences/`)
- **Bricolage Grotesque** (instances statiques générées depuis la police variable :
  `Bricolage` = Bold 700 opsz 24, `BricolageHeavy` = ExtraBold 800 opsz 72) : affiche, titres, boutons.
- **Inter** (`Inter`, `InterMedium`, `InterSemiBold`) : texte.
- **Chiffres tabulaires** : `Controls/NumberLabel` + handler Android `FontFeatureSettings = "tnum"`
  (quantités, portions, minuteur, numéros d'étape ne « dansent » pas).

Styles nommés par usage (`Resources/Styles/AppStyles.xaml`) :

| Style | Police / taille | Usage |
|---|---|---|
| `Poster` | Bricolage ExtraBold 32 | titre de recette |
| `StepNumber` | Bricolage ExtraBold 112, aubergine | numéro d'étape (mode cuisson) |
| `Title` | Bricolage Bold 26 | titre de page (`PageHeader`) |
| `Heading` | Bricolage Bold 19 (titre de niveau 2) | section |
| `CardTitle` | Bricolage Bold 16,5 | titre de carte |
| `Eyebrow` / `EyebrowAccent` | Inter SemiBold 11, capitales espacées | surtitres, rayons |
| `Body`, `BodyStrong`, `StepText` (23), `Caption` (13) | Inter | texte |
| `Quantity`, `QuantityMute`, `Counter`, `Timer` (48) | sur `NumberLabel` | chiffres |

Boutons : style implicite = action principale (safran) ; `SecondaryButton`, `QuietButton`,
`DangerButton`, `CompactButton`, `LargeButton`, `LargeQuietButton`, `ChipAction`. Les libellés
passent à la ligne (police 200 %, écran étroit) au lieu d'être coupés.

## Jetons de dimension — `Resources/Styles/Tokens.xaml`
- Espacements `Space4/8/12/16/24/32`, marges nommées (`PagePadding`, `Gutter`, `GutterBand`…).
- Rayons : carte 16, champ 12, bouton 14, chip en pilule.
- Tailles : cible tactile **48**, bouton 50 (64 en mode cuisson), champ 46, bols 12/18/24/30.
- Icônes au trait (grille 24, tracé 1,9) : clés `Icon…`, dessinées par `Controls/Icon.cs`.
- Les formes sont créées par contrôle (`Tokens.Rounded`) : une instance de forme ne se partage
  pas entre deux `Border`.

## Contrôles maison — `Controls/`
Tous accessibles : description TalkBack, rôle Android (`A11y.Role` → `Platforms/Android/Handlers.cs`),
activation au double toucher (`IActivatable`), cible ≥ 48.

| Contrôle | Remplace | Rôle TalkBack |
|---|---|---|
| `PageHeader` | barre de navigation Shell + ToolbarItems + menu ⋮ | titre de niveau 1 |
| `IconButton` | ToolbarItem, boutons « ✕ » en texte | bouton |
| `Field` (+ `Error`, `Suffix`, `ShowChevron`, `Leading`) | Entry / Editor / Picker soulignés | champ libellé |
| `PortionStepper` | Stepper natif gris | un nœud « 4 portions » + 2 boutons |
| `BowlCheck` | CheckBox | case à cocher, état annoncé |
| `ToggleChip` | 3 copies de chips à bascule | bouton bascule |
| `Segmented` | — (Réglages) | case d'option |
| `ToggleSwitch` | Switch Material | bouton bascule |
| `Bowl`, `Icon` | pastilles `Border`, glyphes texte | décoratifs (hors arbre) |
| `NumberLabel` | Label | — |

Handlers Android : Entry / Editor / Picker sans soulignement ni fond Material ; onglets Shell
au rendu maison (`PopoteShellRenderer` : pas de pilule ni d'ondulation, libellés Inter,
icônes `Resources/Images/tab_*.svg`).

## Mouvement — `Motion/`
Jetons (`MotionTokens`) : Enter 420 ms `cubic-bezier(.2,.7,.2,1)`, Transition 480 ms
`(.65,0,.35,1)`, Release 560 ms `(.2,.9,.2,1.2)` ; décalage 50 ms par élément (8 au plus),
40 ms par lettre. Niveau (`MotionLevels.Of`, testé) : **Full** (Ambiance Expressive),
**Reduced** (Sobre : fondus de 150 ms), **None** (réglage Android « Supprimer les animations »,
toujours prioritaire, relu au retour au premier plan). Toute animation est déclenchée par un
événement, annulable, jamais en boucle. Haptique centralisée (`Haptics.Tick` / `Confirm`),
coupée par le réglage Vibrations. Les « moments » (jalon 1.2.0) s'appuient sur ces briques.

## Accessibilité (plancher non négociable)
- Une phrase par élément composé (« Farine, 250 grammes, rayon Épicerie », « Lait, 50 centilitres »),
  enfants hors de l'arbre ; formes parlées des unités (`QuantityFormat.SpokenLabel`, testé).
- Titres de page en niveau 1, sections en niveau 2.
- Cibles ≥ 48 ; hauteurs minimales, jamais fixes, pour ce qui contient du texte ; vérifié à la
  police 200 % et en densité d'écran forcée.
- Le minuteur annonce sa fin une seule fois, jamais chaque seconde.

## Voix et typographie française
Verbes d'action explicites (« Enregistrer », « Cuisiner »), erreurs qui disent quoi corriger,
apostrophe ’, guillemets « » avec insécables, espace fine insécable avant ; ! ?, insécable
entre nombre et unité (« 250 g ») — vérifié par `ResxTests`.

## Dépendances
Aucune bibliothèque d'interface : MAUI seul (+ polices OFL embarquées). CommunityToolkit.Maui,
Lottie, UraniumUI : non retenus (le système maison couvre le besoin).
