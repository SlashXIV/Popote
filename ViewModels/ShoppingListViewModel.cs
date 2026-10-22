using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Popote.Resources.Strings;
using Popote.Services;

namespace Popote.ViewModels;

// ViewModel de l'onglet « Courses » : liste de courses PERSISTANTE.
// - « Générer » (re)construit les articles issus des recettes cochées, en gardant les manuels.
// - Ajout d'article à la main, coche persistée, retrait, vidage.
public partial class ShoppingListViewModel : ObservableObject
{
    private readonly RecipeService _service;

    public ShoppingListViewModel(RecipeService service) => _service = service;

    // Recettes sélectionnables (haut de la page).
    public ObservableCollection<SelectableRecipeViewModel> Recipes { get; } = new();

    // Liste de courses persistante, groupée par rayon.
    public ObservableCollection<ShoppingAisle> ShoppingList { get; } = new();

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool hasItems;

    [ObservableProperty]
    private string newItemText = string.Empty;

    // Section « À partir de recettes » repliée par défaut (la liste est l'essentiel).
    [ObservableProperty]
    private bool isPickingRecipes;

    // « 5 sur 8 à prendre »
    [ObservableProperty]
    private string remainingLabel = string.Empty;

    [RelayCommand]
    private void ToggleRecipePicker() => IsPickingRecipes = !IsPickingRecipes;

    private void UpdateRemaining()
    {
        var all = ShoppingList.SelectMany(g => g).ToList();
        RemainingLabel = all.Count == 0 ? string.Empty
            : string.Format(AppResources.Shopping_Remaining, all.Count(i => !i.IsChecked), all.Count);
    }

    private async Task OnItemToggledAsync(int id, bool isChecked)
    {
        await _service.SetItemCheckedAsync(id, isChecked);
        UpdateRemaining();
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            Recipes.Clear();
            foreach (var r in await _service.GetRecipesAsync())
                Recipes.Add(new SelectableRecipeViewModel(r));

            await RefreshListAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Recharge la liste persistante depuis la base et la regroupe par rayon.
    private async Task RefreshListAsync()
    {
        var items = await _service.GetShoppingListAsync();

        ShoppingList.Clear();
        foreach (var group in items.GroupBy(i => i.Aisle ?? AppResources.Aisle_Other))
            ShoppingList.Add(new ShoppingAisle(
                group.Key,
                group.Select(i => new ShoppingItemViewModel(i, OnItemToggledAsync))));

        HasItems = items.Count > 0;
        UpdateRemaining();
    }

    // Reconstruit les articles issus des recettes cochées (conserve les manuels).
    [RelayCommand]
    private async Task GenerateAsync()
    {
        var ids = Recipes.Where(r => r.IsSelected).Select(r => r.Recipe.Id).ToList();
        await _service.RebuildShoppingListAsync(ids);
        IsPickingRecipes = false;
        await RefreshListAsync();
    }

    [RelayCommand]
    private async Task AddManualItemAsync()
    {
        if (string.IsNullOrWhiteSpace(NewItemText)) return;
        await _service.AddManualItemAsync(NewItemText);
        NewItemText = string.Empty;
        await RefreshListAsync();
    }

    [RelayCommand]
    private async Task RemoveItemAsync(ShoppingItemViewModel? item)
    {
        if (item is null) return;
        await _service.RemoveShoppingItemAsync(item.Id);
        await RefreshListAsync();
    }

    [RelayCommand]
    private async Task ClearAsync()
    {
        var confirm = await Shell.Current.DisplayAlertAsync(
            AppResources.Shopping_ClearTitle, AppResources.Shopping_ClearMessage,
            AppResources.Shopping_ClearAccept, AppResources.Action_Cancel);
        if (!confirm) return;
        await _service.ClearShoppingListAsync();
        await RefreshListAsync();
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
