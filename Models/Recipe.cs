namespace Popote.Models;

// Une recette. C'est l'entité centrale de l'app.
public class Recipe
{
    public int Id { get; set; }                       // clé primaire (auto-incrémentée par SQLite)
    public string Title { get; set; } = string.Empty; // titre affiché dans la liste
    public string? Instructions { get; set; }         // étapes de préparation (texte libre)
    public int Servings { get; set; } = 2;            // nombre de portions de BASE (sert au recalcul)
    public int? PrepMinutes { get; set; }             // temps de préparation (optionnel)
    public int? CookMinutes { get; set; }             // temps de cuisson (optionnel)
    public string? PhotoPath { get; set; }            // chemin local de la photo du plat (étape 4)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // --- Relations (EF Core les remplit via .Include(...) dans les requêtes) ---

    // Les lignes "ingrédient + quantité" de la recette.
    public List<RecipeIngredient> Ingredients { get; set; } = new();

    // Les tags associés (végé, rapide, dessert...).
    public List<RecipeTag> RecipeTags { get; set; } = new();
}
