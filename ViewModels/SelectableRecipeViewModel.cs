using CommunityToolkit.Mvvm.ComponentModel;
using RecettesApp.Models;

namespace RecettesApp.ViewModels;

// Enveloppe une recette pour la sélection multiple de la liste de courses :
// on ajoute juste un état "coché" sans toucher à l'entité Recipe.
public partial class SelectableRecipeViewModel : ObservableObject
{
    public Recipe Recipe { get; }

    public SelectableRecipeViewModel(Recipe recipe) => Recipe = recipe;

    public string Title => Recipe.Title;

    [ObservableProperty]
    private bool isSelected;
}
