using Microsoft.EntityFrameworkCore;
using RecettesApp.Data;
using RecettesApp.Models;

namespace RecettesApp.Services;

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

    // --- Lecture : liste (avec recherche optionnelle par titre) ---
    public async Task<List<Recipe>> GetRecipesAsync(string? search = null)
    {
        using var db = await _factory.CreateDbContextAsync();

        var query = db.Recipes
            .Include(r => r.RecipeTags).ThenInclude(rt => rt.Tag) // on charge aussi les tags
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(r => r.Title.Contains(search));

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
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

    // --- Création / mise à jour des champs principaux ET des lignes d'ingrédients ---
    // Les lignes (nom + quantité + unité) arrivent sous forme de DTO (IngredientInput).
    // Le service se charge du catalogue : pour chaque nom, on réutilise l'Ingredient
    // existant (recherche insensible à la casse) ou on le crée.
    public async Task SaveRecipeAsync(Recipe recipe, IReadOnlyList<IngredientInput> ingredients)
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
            // Recette existante : on charge aussi ses lignes pour pouvoir les remplacer
            var existing = await db.Recipes
                .Include(r => r.Ingredients)
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
            var ingredient = await GetOrCreateIngredientAsync(db, cache, name);

            target.Ingredients.Add(new RecipeIngredient
            {
                Ingredient = ingredient,
                Quantity = input.Quantity,
                Unit = string.IsNullOrWhiteSpace(input.Unit) ? null : input.Unit!.Trim()
            });
        }

        await db.SaveChangesAsync();
    }

    // Trouve l'ingrédient par nom (insensible à la casse) ou le crée.
    // EF l'insérera au SaveChanges car il est référencé par une nouvelle RecipeIngredient.
    private static async Task<Ingredient> GetOrCreateIngredientAsync(
        AppDbContext db, Dictionary<string, Ingredient> cache, string name)
    {
        if (cache.TryGetValue(name, out var cached))
            return cached;

        var lower = name.ToLower();
        var existing = await db.Ingredients.FirstOrDefaultAsync(i => i.Name.ToLower() == lower);

        var ingredient = existing ?? new Ingredient { Name = name };
        cache[name] = ingredient;
        return ingredient;
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

        return lines
            .GroupBy(ri => new { ri.Ingredient.Name, ri.Ingredient.Aisle, ri.Unit })
            .Select(g => new ShoppingItem(
                Name: g.Key.Name,
                Aisle: g.Key.Aisle ?? "Divers",
                Quantity: g.Sum(x => x.Quantity),
                Unit: g.Key.Unit))
            .OrderBy(s => s.Aisle)
            .ThenBy(s => s.Name)
            .ToList();
    }
}

// Ligne de liste de courses (résultat agrégé, pas une entité en base).
public record ShoppingItem(string Name, string Aisle, double Quantity, string? Unit)
{
    // Libellé prêt à afficher (ex : "400 g", "2 pièce", ou juste "1,5" sans unité).
    public string QuantityLabel =>
        string.IsNullOrWhiteSpace(Unit) ? Quantity.ToString() : $"{Quantity} {Unit}";
}

// Saisie d'une ligne d'ingrédient venant de l'UI (pas une entité en base).
// Le service la transforme en Ingredient (catalogue) + RecipeIngredient.
public record IngredientInput(string Name, double Quantity, string? Unit);
