# Convention de commit — RecettesApp

> Norme à appliquer à **chaque** commit, sans exception. But : un historique
> lisible, professionnel et homogène, exploitable pour générer le CHANGELOG.

## Format
```
<type>(<scope optionnel>): <résumé>

<corps optionnel>

<footer optionnel>
```

## Règles du résumé (1re ligne)
- **Type** parmi : `feat`, `fix`, `docs`, `refactor`, `chore`, `test`, `perf`, `build`, `ci`.
- **Scope** optionnel, en minuscules, entre parenthèses : `(recipes)`, `(data)`, `(ui)`, `(build)`.
- **Verbe à l'impératif présent**, en français : « ajoute », « corrige », « supprime » — pas « ajouté » ni « ajout de ».
- Minuscule en début de résumé, **pas de point final**, **≤ 72 caractères**.

## Corps (optionnel)
- Explique le **pourquoi**, pas le comment (le diff montre déjà le comment).
- Lignes ≤ 72 caractères, séparé du résumé par une ligne vide.
- Référence un jalon de la ROADMAP quand c'est pertinent.

## Footer (optionnel)
- Changement cassant : `BREAKING CHANGE: <description>`.
- Référence de ticket si un jour il y en a : `Refs #123`.

## Règles d'or
- **Un commit = un seul changement cohérent.** Pas de commit fourre-tout.
- **Aucune mention d'outil ou d'assistant** dans le message (ni « Claude », ni
  « généré par », ni ligne `Co-Authored-By` d'un assistant). Les commits sont
  signés du seul auteur humain.
- Pas de secrets, clés ni données perso dans le message.

## Exemples
```
feat(recipes): ajoute l'édition des ingrédients d'une recette

Permet d'ajouter/supprimer des lignes (ingrédient + quantité + unité)
depuis la page d'édition. Couvre le point 1 de la ROADMAP.
```
```
fix(data): corrige la division par zéro dans ServingsScaler
```
```
docs: documente la procédure de build Android
```
```
chore(build): épingle SQLitePCLRaw pour corriger l'alerte NU1903
```
