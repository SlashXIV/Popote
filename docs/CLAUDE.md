# CLAUDE.md — RecettesApp

Instructions persistantes pour Claude Code, lues à chaque session.
Écrites à l'impératif = ce sont des règles, pas des suggestions.

@ROADMAP.md

## Projet
- App mobile Android (d'abord) de gestion de recettes de cuisine, usage personnel.
- But réel : monter en compétence sur .NET, **à parts égales côté données et côté logiciel**.
- Stack : .NET MAUI (UI/XAML) + EF Core / SQLite (données) + CommunityToolkit.Mvvm (MVVM).
- Installation et arborescence détaillées : voir `README.md` (ne pas le dupliquer ici).

## Commandes
- Build + run Android : `dotnet build -t:Run -f net10.0-android` (adapter le TFM à la version .NET installée).
- Restaurer : `dotnet restore`.
- Tests : à mettre en place (voir ROADMAP).

## Architecture (à respecter)
- MVVM strict : une Page (View) ↔ un ViewModel ; la View ne contient pas de logique métier.
- Un ViewModel NE touche JAMAIS la base : il passe par un service (ex. `RecipeService`).
- Accès EF Core via `IDbContextFactory<AppDbContext>` ; un `DbContext` court par opération (`using`).
- Enregistrer tout nouveau service / ViewModel / Page dans `MauiProgram.cs` (injection de dépendances).
- Navigation via Shell + routes (`Routing.RegisterRoute` dans `AppShell.xaml.cs`).

## Conventions de code
- Commentaires et libellés d'interface en **français**.
- C# : nullable activé ; `async`/`await` pour toute I/O ; suffixe `Async` sur les méthodes asynchrones.
- MVVM : utiliser les générateurs CommunityToolkit (`[ObservableProperty]`, `[RelayCommand]`) ; classes `partial`.
- XAML : activer `x:DataType` (bindings compilés) sur chaque page et chaque DataTemplate.

## Workflow obligatoire (à chaque feature)
1. Avant de coder : proposer un court plan et le confronter à `ROADMAP.md`. Ne pas s'écarter du but.
2. Créer / mettre à jour `docs/<feature>.md` à partir de `docs/_TEMPLATE.md` (objectif, comportement, fichiers touchés, choix techniques).
3. Mettre à jour `CHANGELOG.md` (section `## [Unreleased]`) à chaque changement notable.
4. Mettre à jour `ROADMAP.md` : cocher le fait, ajouter ce qui émerge.
5. Commits : suivre **`docs/commit-convention.md`** (Conventional Commits en français, impératif présent, un commit = un changement cohérent). **Ne jamais mentionner Claude / un outil dans un message de commit (pas de `Co-Authored-By` d'assistant).**

## Definition of Done
- Le projet **compile** (build réellement lancé, jamais supposé).
- `docs/<feature>.md`, `CHANGELOG.md` et `ROADMAP.md` à jour.
- Pas de `TODO` non tracké (sinon l'ajouter à la roadmap).

## Garde-fous anti-hallucination
- Ne pas inventer d'API .NET / MAUI / EF Core. En cas de doute, vérifier sur learn.microsoft.com AVANT d'écrire.
- Vérifier le nom et la version d'un package NuGet avant de l'ajouter ; ne jamais inventer un package.
- Ne jamais affirmer que « ça compile » ou « ça marche » sans avoir exécuté le build.
- Si une info manque (chemin, intention, choix produit) : POSER LA QUESTION plutôt que supposer.
- Tout changement multi-fichiers ou touchant l'architecture : présenter le plan et attendre validation.
- Ne jamais `commit` ni `push` sans confirmation explicite.

## Ne jamais faire
- Mettre des secrets, clés ou données perso dans le code ou les fichiers versionnés.
- Modifier les dossiers générés (`bin/`, `obj/`).
- Réécrire massivement du code existant qui fonctionne sans qu'on l'ait demandé.
