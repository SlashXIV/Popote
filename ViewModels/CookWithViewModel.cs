using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Popote.Models;
using Popote.Services;

namespace Popote.ViewModels;

// « Cuisiner avec… » : on coche des ingrédients disponibles et on liste les
// recettes qui les contiennent tous.
public partial class CookWithViewModel : ObservableObject
{
    private readonly RecipeService _service;

    public CookWithViewModel(RecipeService service) => _service = service;

    // Ingrédients du catalogue en puces à bascule.
    public ObservableCollection<TagToggleViewModel> Ingredients { get; } = new();

    // Recettes correspondant à la sélection.
    public ObservableCollection<Recipe> Results { get; } = new();

    [ObservableProperty]
    private bool hasSelection;

    [RelayCommand]
    private async Task LoadAsync()
    {
        // Recharge le catalogue en conservant les cases cochées.
        var selected = Ingredients.Where(i => i.IsSelected).Select(i => i.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Ingredients.Clear();
        foreach (var item in await _service.GetIngredientCatalogAsync())
            Ingredients.Add(new TagToggleViewModel(item.Name, selected.Contains(item.Name)));

        await SearchAsync();
    }

    // Bascule un ingrédient puis relance la recherche.
    [RelayCommand]
    private async Task ToggleAsync(TagToggleViewModel? ingredient)
    {
        if (ingredient is null) return;
        ingredient.IsSelected = !ingredient.IsSelected;
        await SearchAsync();
    }

    private async Task SearchAsync()
    {
        var selected = Ingredients.Where(i => i.IsSelected).Select(i => i.Name).ToList();
        HasSelection = selected.Count > 0;

        Results.Clear();
        if (!HasSelection) return;

        foreach (var recipe in await _service.FindRecipesByIngredientsAsync(selected))
            Results.Add(recipe);
    }

    [RelayCommand]
    private async Task GoToDetailAsync(Recipe? recipe)
    {
        if (recipe is null) return;
        await Shell.Current.GoToAsync($"RecipeDetailPage?id={recipe.Id}");
    }
}
