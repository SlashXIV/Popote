using Popote.Models;

namespace Popote.Services;

// Logique PURE d'agrégation de la liste de courses (sans base de données),
// extraite pour être testable unitairement.
public static class ShoppingListBuilder
{
    // Regroupe les lignes par (nom + rayon + unité), additionne les quantités,
    // trie par rayon puis nom. Rayon absent -> « Divers ».
    public static List<ShoppingItem> Aggregate(IEnumerable<RecipeIngredient> lines)
        => lines
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

// Ligne de liste de courses (résultat agrégé, pas une entité en base).
public record ShoppingItem(string Name, string Aisle, double Quantity, string? Unit)
{
    // Libellé prêt à afficher (ex : "400 g", "2 pièce", ou juste "1,5" sans unité).
    public string QuantityLabel =>
        string.IsNullOrWhiteSpace(Unit) ? Quantity.ToString() : $"{Quantity} {Unit}";
}
