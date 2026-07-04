using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Popote.Models;
using Popote.Services;

namespace Popote.ViewModels;

// ViewModel de la page liste.
// "partial" est obligatoire : CommunityToolkit.Mvvm génère du code
// (les propriétés et les commandes) à la compilation.
public partial class RecipeListViewModel : ObservableObject
{
    private readonly RecipeService _service;

    public RecipeListViewModel(RecipeService service) => _service = service;

    // Collection observable : la UI se met à jour automatiquement quand on ajoute/retire.
    public ObservableCollection<Recipe> Recipes { get; } = new();

    // Tags de filtre (puces à bascule). Filtre en ET : une recette doit porter
    // tous les tags actifs.
    public ObservableCollection<TagToggleViewModel> FilterTags { get; } = new();

    [ObservableProperty]
    private bool hasTags;

    // [ObservableProperty] sur le champ "searchText" génère une propriété "SearchText"
    // qui notifie l'UI à chaque changement.
    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private bool isBusy;

    // [RelayCommand] sur "LoadAsync" génère une commande "LoadCommand"
    // (le suffixe "Async" est retiré automatiquement).
    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            // Rafraîchit les tags de filtre en conservant les sélections actives.
            var selected = FilterTags.Where(t => t.IsSelected).Select(t => t.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var allTags = await _service.GetTagsAsync();
            FilterTags.Clear();
            foreach (var name in allTags)
                FilterTags.Add(new TagToggleViewModel(name, selected.Contains(name)));
            HasTags = FilterTags.Count > 0;

            var activeTags = FilterTags.Where(t => t.IsSelected).Select(t => t.Name).ToList();

            Recipes.Clear();
            var list = await _service.GetRecipesAsync(SearchText, activeTags);
            foreach (var r in list)
                Recipes.Add(r);
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Active/désactive un tag de filtre puis recharge la liste.
    [RelayCommand]
    private async Task ToggleFilterAsync(TagToggleViewModel? tag)
    {
        if (tag is null) return;
        tag.IsSelected = !tag.IsSelected;
        await LoadAsync();
    }

    // Navigation vers la page d'édition.
    // recipe == null  -> création (bouton « Ajouter ») ; sinon -> édition.
    [RelayCommand]
    private async Task GoToEditAsync(Recipe? recipe)
    {
        var route = recipe is null
            ? "RecipeEditPage"                       // route enregistrée dans AppShell
            : $"RecipeEditPage?id={recipe.Id}";      // on passe l'id en paramètre
        await Shell.Current.GoToAsync(route);
    }

    // Tap sur une recette -> page de consultation (détail).
    [RelayCommand]
    private async Task GoToDetailAsync(Recipe? recipe)
    {
        if (recipe is null) return;
        await Shell.Current.GoToAsync($"RecipeDetailPage?id={recipe.Id}");
    }
}
