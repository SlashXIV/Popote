using RecettesApp.ViewModels;

namespace RecettesApp.Views;

public partial class ShoppingListPage : ContentPage
{
    private readonly ShoppingListViewModel _vm;

    public ShoppingListPage(ShoppingListViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    // À chaque affichage de l'onglet, on rafraîchit la liste des recettes
    // (une recette ajoutée ailleurs doit apparaître ici).
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadCommand.ExecuteAsync(null);
    }
}
