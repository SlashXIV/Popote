namespace RecettesApp.Models;

// Un tag/étiquette pour filtrer (ex : "végé", "rapide", "batch cooking").
public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public List<RecipeTag> RecipeTags { get; set; } = new();
}
