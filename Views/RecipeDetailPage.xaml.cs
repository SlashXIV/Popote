using Popote.ViewModels;

namespace Popote.Views;

public partial class RecipeDetailPage : ContentPage
{
    private readonly RecipeDetailViewModel _vm;
    private bool _opened;

    public RecipeDetailPage(RecipeDetailViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    // Au retour sur la page (après une modification), la recette affichée est relue.
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (_opened) _ = _vm.RefreshAsync();
        _opened = true;
    }
}
