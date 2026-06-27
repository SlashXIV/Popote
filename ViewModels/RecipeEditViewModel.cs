using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RecettesApp.Models;
using RecettesApp.Services;

namespace RecettesApp.ViewModels;

// [QueryProperty] récupère le paramètre "id" passé dans l'URL de navigation
// (ex : "RecipeEditPage?id=3") et l'injecte dans la propriété RecipeId.
[QueryProperty(nameof(RecipeId), "id")]
public partial class RecipeEditViewModel : ObservableObject
{
    private readonly RecipeService _service;

    public RecipeEditViewModel(RecipeService service) => _service = service;

    [ObservableProperty]
    private int recipeId;            // 0 = création, >0 = édition

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private string? instructions;

    [ObservableProperty]
    private int servings = 2;

    // Méthode partielle générée : appelée automatiquement quand RecipeId change.
    // Si on édite une recette existante, on charge ses données.
    partial void OnRecipeIdChanged(int value)
    {
        if (value > 0)
            _ = LoadAsync(value);
    }

    private async Task LoadAsync(int id)
    {
        var r = await _service.GetRecipeAsync(id);
        if (r is null) return;

        Title = r.Title;
        Instructions = r.Instructions;
        Servings = r.Servings;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Title))
            return; // garde-fou minimal : pas de recette sans titre

        var recipe = new Recipe
        {
            Id = RecipeId,
            Title = Title.Trim(),
            Instructions = Instructions,
            Servings = Servings
        };

        await _service.SaveRecipeAsync(recipe);
        await Shell.Current.GoToAsync(".."); // ".." = retour à la page précédente (la liste)
    }
}
