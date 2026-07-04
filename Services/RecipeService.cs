using Microsoft.EntityFrameworkCore;
using Popote.Data;
using Popote.Models;

namespace Popote.Services;

// Couche "métier" : centralise toutes les requêtes.
// Les ViewModels NE touchent JAMAIS la base directement, ils passent par ici.
//
// On reçoit un IDbContextFactory plutôt qu'un DbContext :
// un DbContext n'est pas thread-safe ni fait pour vivre longtemps.
// On en crée donc un neuf, court, à chaque opération (pattern recommandé en MAUI).
public class RecipeService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public RecipeService(IDbContextFactory<AppDbContext> factory) => _factory = factory;

    // --- Lecture : liste (recherche par titre + filtre par tags, cumul ET + tri) ---
    public async Task<List<Recipe>> GetRecipesAsync(
        string? search = null, IReadOnlyList<string>? tags = null, RecipeSort sort = RecipeSort.Favorite)
    {
        using var db = await _factory.CreateDbContextAsync();

        var query = db.Recipes
            .Include(r => r.RecipeTags).ThenInclude(rt => rt.Tag) // on charge aussi les tags
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(r => r.Title.Contains(search));

        // Filtre ET : la recette doit porter CHACUN des tags sélectionnés.
        if (tags is not null)
        {
            foreach (var tag in tags)
            {
                var name = tag; // capture par itération
                query = query.Where(r => r.RecipeTags.Any(rt => rt.Tag.Name == name));
            }
        }

        query = sort switch
        {
            RecipeSort.Title => query.OrderBy(r => r.Title),
            RecipeSort.Time => query.OrderBy(r => (r.PrepMinutes ?? 0) + (r.CookMinutes ?? 0)),
            RecipeSort.Recent => query.OrderByDescending(r => r.CreatedAt),
            _ => query.OrderByDescending(r => r.IsFavorite).ThenByDescending(r => r.CreatedAt), // Favoris
        };

        return await query.ToListAsync();
    }

    // Bascule rapidement l'état favori d'une recette (depuis la page détail).
    public async Task SetFavoriteAsync(int id, bool isFavorite)
    {
        using var db = await _factory.CreateDbContextAsync();
        var recipe = await db.Recipes.FindAsync(id);
        if (recipe is null) return;
        recipe.IsFavorite = isFavorite;
        await db.SaveChangesAsync();
    }

    // --- Lecture : recettes contenant TOUS les ingrédients donnés (cumul ET) ---
    // « Qu'est-ce que je peux cuisiner avec X, Y ? »
    public async Task<List<Recipe>> FindRecipesByIngredientsAsync(IReadOnlyList<string> ingredientNames)
    {
        using var db = await _factory.CreateDbContextAsync();

        var query = db.Recipes.AsQueryable();
        foreach (var ingredient in ingredientNames)
        {
            var name = ingredient; // capture par itération
            query = query.Where(r => r.Ingredients.Any(ri => ri.Ingredient.Name == name));
        }

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    // --- Lecture : tous les tags (pour l'édition et le filtre) ---
    public async Task<List<string>> GetTagsAsync()
    {
        using var db = await _factory.CreateDbContextAsync();
        return await db.Tags.OrderBy(t => t.Name).Select(t => t.Name).ToListAsync();
    }

    // --- Lecture : une recette complète (avec ses ingrédients) ---
    public async Task<Recipe?> GetRecipeAsync(int id)
    {
        using var db = await _factory.CreateDbContextAsync();

        return await db.Recipes
            .Include(r => r.Ingredients).ThenInclude(ri => ri.Ingredient)
            .Include(r => r.RecipeTags).ThenInclude(rt => rt.Tag)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    // --- Lecture : catalogue d'ingrédients (nom + rayon) pour les suggestions ---
    public async Task<List<IngredientCatalogItem>> GetIngredientCatalogAsync()
    {
        using var db = await _factory.CreateDbContextAsync();
        return await db.Ingredients
            .OrderBy(i => i.Name)
            .Select(i => new IngredientCatalogItem(i.Name, i.Aisle))
            .ToListAsync();
    }

    // --- Création / mise à jour des champs principaux ET des lignes d'ingrédients ---
    // Les lignes (nom + quantité + unité) arrivent sous forme de DTO (IngredientInput).
    // Le service se charge du catalogue : pour chaque nom, on réutilise l'Ingredient
    // existant (recherche insensible à la casse) ou on le crée.
    public async Task SaveRecipeAsync(Recipe recipe, IReadOnlyList<IngredientInput> ingredients, IReadOnlyList<string> tags)
    {
        using var db = await _factory.CreateDbContextAsync();

        Recipe target;
        if (recipe.Id == 0)
        {
            // Nouvelle recette
            target = new Recipe();
            db.Recipes.Add(target);
        }
        else
        {
            // Recette existante : on charge ses lignes ET ses tags pour les remplacer
            var existing = await db.Recipes
                .Include(r => r.Ingredients)
                .Include(r => r.RecipeTags)
                .FirstOrDefaultAsync(r => r.Id == recipe.Id);
            if (existing is null) return;
            target = existing;
        }

        // Champs scalaires
        target.Title = recipe.Title;
        target.Instructions = recipe.Instructions;
        target.Servings = recipe.Servings;
        target.PrepMinutes = recipe.PrepMinutes;
        target.CookMinutes = recipe.CookMinutes;
        target.PhotoPath = recipe.PhotoPath;
        target.IsFavorite = recipe.IsFavorite;
        target.Notes = recipe.Notes;

        // Lignes d'ingrédients : stratégie simple et fiable pour un usage perso —
        // on vide les lignes existantes (EF supprime les RecipeIngredient orphelins
        // par cascade) puis on les recrée à partir des saisies.
        target.Ingredients.Clear();

        // Cache local pour ne pas créer deux fois le même ingrédient dans une même sauvegarde.
        var cache = new Dictionary<string, Ingredient>(StringComparer.OrdinalIgnoreCase);

        foreach (var input in ingredients)
        {
            if (string.IsNullOrWhiteSpace(input.Name))
                continue; // on ignore les lignes sans nom

            var name = input.Name.Trim();
            var ingredient = await GetOrCreateIngredientAsync(db, cache, name, input.Aisle);

            target.Ingredients.Add(new RecipeIngredient
            {
                Ingredient = ingredient,
                Quantity = input.Quantity,
                Unit = string.IsNullOrWhiteSpace(input.Unit) ? null : input.Unit!.Trim()
            });
        }

        // Tags : même stratégie (on vide et on recrée les liaisons).
        target.RecipeTags.Clear();
        var tagCache = new Dictionary<string, Tag>(StringComparer.OrdinalIgnoreCase);
        foreach (var raw in tags)
        {
            if (string.IsNullOrWhiteSpace(raw))
                continue;
            var tag = await GetOrCreateTagAsync(db, tagCache, raw.Trim());
            target.RecipeTags.Add(new RecipeTag { Recipe = target, Tag = tag });
        }

        await db.SaveChangesAsync();
    }

    // Trouve le tag par nom (insensible à la casse) ou le crée.
    private static async Task<Tag> GetOrCreateTagAsync(
        AppDbContext db, Dictionary<string, Tag> cache, string name)
    {
        if (cache.TryGetValue(name, out var tag))
            return tag;

        var lower = name.ToLower();
        var existing = await db.Tags.FirstOrDefaultAsync(t => t.Name.ToLower() == lower);
        tag = existing ?? new Tag { Name = name };
        cache[name] = tag;
        return tag;
    }

    // Trouve l'ingrédient par nom (insensible à la casse) ou le crée.
    // Met à jour son rayon si fourni (le rayon est une propriété du catalogue,
    // partagée entre toutes les recettes qui utilisent cet ingrédient).
    // EF l'insérera/le mettra à jour au SaveChanges.
    private static async Task<Ingredient> GetOrCreateIngredientAsync(
        AppDbContext db, Dictionary<string, Ingredient> cache, string name, string? aisle)
    {
        var canonical = Capitalize(name);
        if (!cache.TryGetValue(canonical, out var ingredient))
        {
            var lower = canonical.ToLower();
            var existing = await db.Ingredients.FirstOrDefaultAsync(i => i.Name.ToLower() == lower);
            ingredient = existing ?? new Ingredient { Name = canonical };
            cache[canonical] = ingredient;
        }

        ingredient.Name = canonical; // normalise la casse (ex. « levure » -> « Levure »)
        if (!string.IsNullOrWhiteSpace(aisle))
            ingredient.Aisle = aisle;

        return ingredient;
    }

    // Met la première lettre en majuscule (le reste inchangé).
    private static string Capitalize(string name)
    {
        var t = name.Trim();
        return t.Length == 0 ? t : char.ToUpper(t[0]) + t[1..];
    }

    // --- Suppression ---
    public async Task DeleteRecipeAsync(int id)
    {
        using var db = await _factory.CreateDbContextAsync();
        var existing = await db.Recipes.FindAsync(id);
        if (existing is null) return;
        db.Recipes.Remove(existing);
        await db.SaveChangesAsync();
    }

    // =========================================================================
    // FEATURE DATA : liste de courses agrégée sur PLUSIEURS recettes.
    // On prend tous les ingrédients des recettes sélectionnées, on les regroupe
    // par (nom + unité), on additionne les quantités, et on trie par rayon.
    // C'est typiquement le genre de requête (GroupBy + Sum) qu'on retrouve en data.
    // =========================================================================
    public async Task<List<ShoppingItem>> BuildShoppingListAsync(IEnumerable<int> recipeIds)
    {
        using var db = await _factory.CreateDbContextAsync();

        var lines = await db.RecipeIngredients
            .Where(ri => recipeIds.Contains(ri.RecipeId))
            .Include(ri => ri.Ingredient)
            .ToListAsync();

        // Agrégation pure déléguée à ShoppingListBuilder (testable unitairement).
        return ShoppingListBuilder.Aggregate(lines);
    }

    // =========================================================================
    // PLANIFICATEUR : repas prévus sur une plage de dates.
    // =========================================================================
    public async Task<List<PlannedMeal>> GetPlannedMealsAsync(DateTime fromInclusive, DateTime toExclusive)
    {
        using var db = await _factory.CreateDbContextAsync();
        return await db.PlannedMeals
            .Where(p => p.Date >= fromInclusive && p.Date < toExclusive)
            .Include(p => p.Recipe)
            .OrderBy(p => p.Date)
            .ToListAsync();
    }

    public async Task AddPlannedMealAsync(DateTime date, int recipeId)
    {
        using var db = await _factory.CreateDbContextAsync();
        db.PlannedMeals.Add(new PlannedMeal { Date = date.Date, RecipeId = recipeId });
        await db.SaveChangesAsync();
    }

    public async Task RemovePlannedMealAsync(int id)
    {
        using var db = await _factory.CreateDbContextAsync();
        var meal = await db.PlannedMeals.FindAsync(id);
        if (meal is null) return;
        db.PlannedMeals.Remove(meal);
        await db.SaveChangesAsync();
    }

    // =========================================================================
    // SAUVEGARDE / RESTAURATION
    // =========================================================================

    // Rapatrie le journal WAL dans le fichier .db3 : la sauvegarde tient en un seul fichier.
    public async Task CheckpointAsync()
    {
        using var db = await _factory.CreateDbContextAsync();
        await db.Database.ExecuteSqlRawAsync("PRAGMA wal_checkpoint(TRUNCATE);");
    }

    // Applique les migrations (utile après restauration d'une base plus ancienne).
    public async Task MigrateAsync()
    {
        using var db = await _factory.CreateDbContextAsync();
        await db.Database.MigrateAsync();
    }

    // =========================================================================
    // LISTE DE COURSES PERSISTANTE
    // =========================================================================
    public async Task<List<ShoppingListItem>> GetShoppingListAsync()
    {
        using var db = await _factory.CreateDbContextAsync();
        return await db.ShoppingListItems
            .OrderBy(i => i.Aisle)
            .ThenBy(i => i.Name)
            .ToListAsync();
    }

    // (Re)génère les articles issus des recettes ; conserve les articles manuels.
    public async Task RebuildShoppingListAsync(IEnumerable<int> recipeIds)
    {
        using var db = await _factory.CreateDbContextAsync();

        var autos = await db.ShoppingListItems.Where(i => !i.IsManual).ToListAsync();
        db.ShoppingListItems.RemoveRange(autos);

        var lines = await db.RecipeIngredients
            .Where(ri => recipeIds.Contains(ri.RecipeId))
            .Include(ri => ri.Ingredient)
            .ToListAsync();

        foreach (var s in ShoppingListBuilder.Aggregate(lines))
            db.ShoppingListItems.Add(new ShoppingListItem
            {
                Name = s.Name,
                Aisle = s.Aisle,
                Quantity = s.Quantity,
                Unit = s.Unit,
                IsManual = false,
                IsChecked = false
            });

        await db.SaveChangesAsync();
    }

    public async Task AddManualItemAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return;
        using var db = await _factory.CreateDbContextAsync();
        db.ShoppingListItems.Add(new ShoppingListItem
        {
            Name = name.Trim(),
            Aisle = "Divers",
            IsManual = true
        });
        await db.SaveChangesAsync();
    }

    public async Task SetItemCheckedAsync(int id, bool isChecked)
    {
        using var db = await _factory.CreateDbContextAsync();
        var item = await db.ShoppingListItems.FindAsync(id);
        if (item is null) return;
        item.IsChecked = isChecked;
        await db.SaveChangesAsync();
    }

    public async Task RemoveShoppingItemAsync(int id)
    {
        using var db = await _factory.CreateDbContextAsync();
        var item = await db.ShoppingListItems.FindAsync(id);
        if (item is null) return;
        db.ShoppingListItems.Remove(item);
        await db.SaveChangesAsync();
    }

    public async Task ClearShoppingListAsync()
    {
        using var db = await _factory.CreateDbContextAsync();
        var all = await db.ShoppingListItems.ToListAsync();
        db.ShoppingListItems.RemoveRange(all);
        await db.SaveChangesAsync();
    }
}

// Critères de tri de la liste des recettes.
public enum RecipeSort { Favorite, Recent, Title, Time }

// Saisie d'une ligne d'ingrédient venant de l'UI (pas une entité en base).
// Le service la transforme en Ingredient (catalogue) + RecipeIngredient.
public record IngredientInput(string Name, double Quantity, string? Unit, string? Aisle);

// Élément du catalogue d'ingrédients (suggestions de saisie).
public record IngredientCatalogItem(string Name, string? Aisle);
