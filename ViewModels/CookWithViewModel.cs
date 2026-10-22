using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Popote.Models;
using Popote.Resources.Strings;
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

    // Les mêmes puces, rangées par rayon pour l'affichage (au lieu d'un mur de chips).
    public ObservableCollection<AisleToggleGroup> Groups { get; } = new();

    // Recettes correspondant à la sélection.
    public ObservableCollection<Recipe> Results { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EmptyMessage))]
    private bool hasSelection;

    public string EmptyMessage => HasSelection ? AppResources.CookWith_NoMatch : AppResources.CookWith_Empty;

    [RelayCommand]
    private async Task LoadAsync()
    {
        // Recharge le catalogue en conservant les cases cochées.
        var selected = Ingredients.Where(i => i.IsSelected).Select(i => i.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Ingredients.Clear();
        Groups.Clear();
        var catalog = await _service.GetIngredientCatalogAsync();
        foreach (var group in catalog.GroupBy(i => string.IsNullOrWhiteSpace(i.Aisle) ? AppResources.Aisle_Other : i.Aisle!)
                                     .OrderBy(g => g.Key))
        {
            var toggles = group.Select(i => new TagToggleViewModel(i.Name, selected.Contains(i.Name))).ToList();
            foreach (var t in toggles) Ingredients.Add(t);
            Groups.Add(new AisleToggleGroup(group.Key, toggles));
        }

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

// Ingrédients d'un même rayon (« Cuisiner avec… »).
public record AisleToggleGroup(string Aisle, IReadOnlyList<TagToggleViewModel> Items);
