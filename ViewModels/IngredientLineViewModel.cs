using CommunityToolkit.Mvvm.ComponentModel;

namespace Popote.ViewModels;

// Une ligne d'ingrédient en cours d'édition dans la page recette.
// La quantité est gardée en texte (QuantityText) pour tolérer la virgule
// décimale ; le parsing se fait à l'enregistrement (cf. RecipeEditViewModel).
public partial class IngredientLineViewModel : ObservableObject
{
    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string quantityText = string.Empty;

    [ObservableProperty]
    private string? unit;

    // Liste d'unités proposée dans le Picker (évite les fautes de frappe et
    // les doublons type « g » / « gr » qui casseraient l'agrégation des courses).
    public IReadOnlyList<string> Units { get; } = UnitOptions;

    public static readonly string[] UnitOptions =
    {
        "g", "kg", "ml", "cl", "L",
        "c. à soupe", "c. à café",
        "pièce", "pincée", "gousse", "tranche", "sachet"
    };
}
