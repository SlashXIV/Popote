using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Popote.Services;

namespace Popote.ViewModels;

// ViewModel de la page "liste de courses".
// 1) On liste les recettes avec une case à cocher.
// 2) "Générer" agrège les ingrédients des recettes cochées (service déjà écrit)
//    et remplit une liste groupée par rayon, affichée dans un CollectionView groupé.
public partial class ShoppingListViewModel : ObservableObject
{
    private readonly RecipeService _service;

    public ShoppingListViewModel(RecipeService service) => _service = service;

    // Recettes sélectionnables (haut de la page).
    public ObservableCollection<SelectableRecipeViewModel> Recipes { get; } = new();

    // Résultat agrégé, groupé par rayon (bas de la page).
    public ObservableCollection<ShoppingAisle> ShoppingList { get; } = new();

    [ObservableProperty]
    private bool isBusy;

    // Pilote l'affichage du titre "Liste de courses" et de l'éventuel message "rien".
    [ObservableProperty]
    private bool hasGenerated;

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            Recipes.Clear();
            var list = await _service.GetRecipesAsync();
            foreach (var r in list)
                Recipes.Add(new SelectableRecipeViewModel(r));
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task GenerateAsync()
    {
        var ids = Recipes.Where(r => r.IsSelected).Select(r => r.Recipe.Id).ToList();

        ShoppingList.Clear();
        HasGenerated = false;

        if (ids.Count == 0)
            return; // rien de coché : on n'affiche pas de liste

        var items = await _service.BuildShoppingListAsync(ids);

        // Le service trie déjà par rayon puis nom : le GroupBy conserve cet ordre.
        foreach (var group in items.GroupBy(i => i.Aisle))
            ShoppingList.Add(new ShoppingAisle(group.Key, group.Select(i => new ShoppingItemViewModel(i))));

        HasGenerated = true;
    }
}

// Groupe d'items partageant le même rayon, pour le CollectionView IsGrouped="True".
// Hériter de List<...> est le modèle attendu par MAUI pour les groupes.
public class ShoppingAisle : List<ShoppingItemViewModel>
{
    public string Aisle { get; }

    public ShoppingAisle(string aisle, IEnumerable<ShoppingItemViewModel> items) : base(items)
        => Aisle = aisle;
}
