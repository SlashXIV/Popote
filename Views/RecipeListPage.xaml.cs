using Popote.ViewModels;

namespace Popote.Views;

public partial class RecipeListPage : ContentPage
{
    private readonly RecipeListViewModel _vm;

    // Le ViewModel est injecté par le conteneur de dépendances (cf. MauiProgram).
    public RecipeListPage(RecipeListViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    // À chaque fois que la page s'affiche (y compris au retour de l'édition),
    // on recharge la liste pour voir les changements.
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadCommand.ExecuteAsync(null);
    }
}
