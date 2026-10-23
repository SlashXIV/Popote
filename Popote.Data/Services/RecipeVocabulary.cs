namespace Popote.Services;

// Listes autorisées d'une recette : unités et rayons proposés à la saisie, imposés à l'import.
// Source unique (formulaire d'édition, consignes pour une IA, analyseur du format texte).
public static class RecipeVocabulary
{
    public static readonly string[] Units =
    {
        "g", "kg", "ml", "cl", "L",
        "c. à soupe", "c. à café",
        "pièce", "pincée", "gousse", "tranche", "sachet"
    };

    public static readonly string[] Aisles =
    {
        "Fruits & légumes", "Crèmerie", "Épicerie",
        "Viande & poisson", "Surgelés", "Divers"
    };

    public const string DefaultAisle = "Divers";
}
