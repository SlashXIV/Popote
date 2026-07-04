namespace Popote.Models;

// Un repas planifié : une recette prévue pour un jour donné.
// (Pas de créneau midi/soir pour l'instant ; un jour peut avoir plusieurs repas.)
public class PlannedMeal
{
    public int Id { get; set; }

    public DateTime Date { get; set; }        // jour planifié (heure ignorée)

    public int RecipeId { get; set; }         // clé étrangère vers Recipe
    public Recipe Recipe { get; set; } = null!;
}
