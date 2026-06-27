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

    // --- Création / mise à jour des champs principaux ---
    // (L'édition des ingrédients viendra à l'étape 1 des "prochaines étapes".)
    public async Task SaveRecipeAsync(Recipe recipe)
    {
        using var db = await _factory.CreateDbContextAsync();

        if (recipe.Id == 0)
        {
            // Nouvelle recette
            db.Recipes.Add(recipe);
        }
        else
        {
            // Recette existante : on met à jour les champs scalaires
            var existing = await db.Recipes.FindAsync(recipe.Id);
            if (existing is null) return;

            existing.Title = recipe.Title;
            existing.Instructions = recipe.Instructions;
            existing.Servings = recipe.Servings;
            existing.PrepMinutes = recipe.PrepMinutes;
            existing.CookMinutes = recipe.CookMinutes;
            existing.PhotoPath = recipe.PhotoPath;
        }

        await db.SaveChangesAsync();
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
public record ShoppingItem(string Name, string Aisle, double Quantity, string? Unit);
