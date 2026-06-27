namespace RecettesApp.Models;

// Un ingrédient "catalogue" (ex : Tomate, Farine, Lait de coco).
// Il est partagé entre plusieurs recettes : on ne le duplique pas.
public class Ingredient
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Rayon en magasin (Fruits & légumes, Épicerie, Frais...).
    // Sert à regrouper la liste de courses de façon pratique.
    public string? Aisle { get; set; }

    // Toutes les lignes de recettes qui utilisent cet ingrédient.
    public List<RecipeIngredient> RecipeIngredients { get; set; } = new();
}
