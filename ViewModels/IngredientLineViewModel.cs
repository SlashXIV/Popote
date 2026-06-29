using CommunityToolkit.Mvvm.ComponentModel;

namespace RecettesApp.ViewModels;

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
}
