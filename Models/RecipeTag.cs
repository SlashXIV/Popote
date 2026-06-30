namespace Popote.Models;

// Jointure many-to-many simple entre Recipe et Tag (pas de données propres).
// La clé primaire est composite (RecipeId + TagId), définie dans AppDbContext.
public class RecipeTag
{
    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;

    public int TagId { get; set; }
    public Tag Tag { get; set; } = null!;
}
