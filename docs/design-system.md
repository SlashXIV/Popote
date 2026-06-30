# Design system — Popote

Brief de design à suivre pour toute UI. Objectif : une app **cohérente, calme et
intentionnelle**, pas une accumulation de contrôles. La cohérence prime sur l'effet.

## Direction visuelle : « épuré, base neutre chaude + accents naturels »

Ambiance claire, calme et confortable (esprit Notion / Bear / Things) — **jamais
de blanc pur ni de noir pur** (contraste adouci pour le confort visuel).
**Base neutre chaude** : fond blanc cassé, cartes papier, filets très légers, texte espresso.
**Palette d'accents « earthy naturals »** (olive, or, sable, teal, brique) utilisée
avec parcimonie : un accent principal (**teal**) pour les actions, les autres pour
**coder la donnée** (pastilles de rayon, tags). La couleur porte du sens, pas de la déco.

**Élément signature** : la carte recette utilise un ruban d'étapes numérotées
(la numérotation est justifiée : une recette EST une séquence) et les ingrédients
sont affichés en « chips » avec une pastille de couleur codant le rayon en magasin.
Cette pastille relie le visuel au modèle de données (`Ingredient.Aisle`) : c'est du
sens, pas de la déco.

## Règles d'or
- Dépenser l'audace à UN seul endroit (l'élément signature) ; garder le reste discret.
- Préférer des filets fins (1px) à des ombres lourdes ; une seule ombre douce pour les cartes.
- Espacement généreux, hiérarchie typographique claire. Le vide fait partie du design.
- Tout passe par des styles (`Styles.xaml`) + des tokens (`AppColors.xaml`). Aucune couleur
  ni taille « en dur » dans une page.
- Clair ET sombre via `AppThemeBinding` sur chaque couleur.

## Couleurs (tokens — voir Resources/Styles/AppColors.xaml)

| Rôle | Clair | Sombre |
|---|---|---|
| Primary / Accent (teal) | `#5B8E7D` | `#7FB3A2` |
| PrimaryContainer (chips) | `#E4EDE9` | `#2A3F38` |
| OnAccent (texte sur accent) | `#FFFFFF` | `#10221C` |
| Background | `#F7F4EE` | `#15191A` |
| Surface / carte | `#FCFAF4` | `#1E2422` |
| TextPrimary | `#2B2926` | `#ECEAE4` |
| TextSecondary | `#6F6A61` | `#A6A29A` |
| Border / filet | `#E8E2D5` | `#2C322F` |

Palette d'accents « earthy naturals » (tags, signature, pastilles) :
Olive `#8CB369` · Or `#F4E285` · Sable `#F4A259` · Teal `#5B8E7D` · Brique `#BC4B51`.

Pastilles de rayon : Fruits & légumes = Olive · Crèmerie = Or · Épicerie = Sable ·
Surgelés = Teal · Divers = gris `#9AA8A2`. Brique réservé aux actions fortes / tags.

> L'app démarre en **thème clair forcé** (`UserAppTheme = Light`). Les tokens sombres
> restent définis pour une réactivation future.

Pastilles de rayon (codage data, mêmes valeurs clair/sombre) :
Fruits & légumes `#6FA85B` · Crèmerie `#E8C25A` · Épicerie `#C98A4B` ·
Surgelés `#5B8DA8` · Divers `#9AA8A2`.

## Typographie
Polices à ajouter dans `Resources/Fonts/` puis à enregistrer dans `MauiProgram`
(`fonts.AddFont(...)`). Toutes dispo gratuitement sur Google Fonts.
- **Display / titres** : `Bricolage Grotesque` (caractère, utilisé avec retenue). PAS de serif.
- **Corps** : `Inter` (lisible, neutre).
- **Données / quantités** : `IBM Plex Mono` (les nombres et unités en mono = lisibles et
  ça souligne discrètement le côté « data »). Optionnel.

Échelle (mobile) :

| Style | Taille | Poids |
|---|---|---|
| Display | 28 | 600 |
| Title | 22 | 600 |
| Subtitle | 17 | 600 |
| Body | 15 | 400 |
| Caption | 13 | 400 |
| Data (mono) | 14 | 500 |

## Espacements & formes
- Échelle d'espacement (base 8) : `4, 8, 12, 16, 24, 32`. Ne pas inventer d'autres valeurs.
- Rayons : carte `16`, champ/bouton `12`, chip `pilule (arrondi total)`.
- Padding de page : `16`. Espacement vertical entre blocs : `16` à `24`.

## Composants (styles à créer dans Styles.xaml)
- `CardBorder` : Surface, rayon 16, filet 1px Border, ombre douce unique.
- `TitleLabel` / `SubtitleLabel` / `BodyLabel` / `CaptionLabel` : applique la police + l'échelle.
- `PrimaryButton` : fond Accent, texte clair, rayon 12, hauteur 48, état pressé plus foncé.
- `Chip` : pilule, fond PrimaryContainer, texte Primary, + pastille rayon optionnelle.
- États vides et chargement soignés (jamais d'écran nu).

## Plancher qualité (non négociable)
- Cibles tactiles ≥ 44px de haut.
- Contraste texte/fond suffisant (lisible en clair et sombre).
- Respecter « réduire les animations » du système ; animations rares et utiles.
- Écran vide = invitation à agir (texte + bouton), jamais un blanc muet.

## Voix / copy de l'interface
- Verbes d'action explicites : « Enregistrer », pas « Soumettre ».
- Un libellé reste le même dans tout le flux (le bouton « Ajouter » mène à un écran cohérent).
- Erreurs : dire ce qui s'est passé et comment corriger, sans s'excuser ni rester vague.

## Librairies autorisées
- **UraniumUI** (Apache 2.0) : finition Material + icônes, sans lock-in. Préféré.
- **CommunityToolkit.Maui** : popups, behaviors, convertisseurs.
- **Syncfusion.Maui.Toolkit** (MIT, gratuit) : charts/datagrid pour le futur dashboard data.
- Pas de kit payant (Grial, Telerik, DevExpress) sans validation explicite.
