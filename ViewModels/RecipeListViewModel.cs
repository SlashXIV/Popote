using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RecettesApp.Models;
using RecettesApp.Services;

namespace RecettesApp.ViewModels;

// ViewModel de la page liste.
// "partial" est obligatoire : CommunityToolkit.Mvvm génère du code
// (les propriétés et les commandes) à la compilation.
public partial class RecipeListViewModel : ObservableObject
{
    private readonly RecipeService _service;

    public RecipeListViewModel(RecipeService service) => _service = service;

    // Collection observable : la UI se met à jour automatiquement quand on ajoute/retire.
    public ObservableCollection<Recipe> Recipes { get; } = new();

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
            Recipes.Clear();
            var list = await _service.GetRecipesAsync(SearchText);
            foreach (var r in list)
                Recipes.Add(r);
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Navigation vers la page d'édition.
    // recipe == null  -> création ; sinon -> édition de la recette existante.
    [RelayCommand]
    private async Task GoToEditAsync(Recipe? recipe)
    {
        var route = recipe is null
            ? "RecipeEditPage"                       // route enregistrée dans AppShell
            : $"RecipeEditPage?id={recipe.Id}";      // on passe l'id en paramètre
        await Shell.Current.GoToAsync(route);
    }
}
