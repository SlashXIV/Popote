namespace Popote.Models;

// Table de jointure entre Recipe et Ingredient.
// Particularité : elle porte des données propres (quantité + unité),
// donc on en fait une vraie entité (et pas une relation many-to-many "nue").
// Exemple : recette "Curry" -> ingrédient "Pois chiches" -> 400 g.
public class RecipeIngredient
{
    public int Id { get; set; }

    public int RecipeId { get; set; }        // clé étrangère vers Recipe
    public Recipe Recipe { get; set; } = null!;

    public int IngredientId { get; set; }    // clé étrangère vers Ingredient
    public Ingredient Ingredient { get; set; } = null!;

    public double Quantity { get; set; }     // ex : 400
    public string? Unit { get; set; }        // ex : "g", "ml", "càs", "pièce"
}
