using Popote.Motion;
using Popote.ViewModels;

namespace Popote.Views;

public partial class RecipeDetailPage : ContentPage
{
    private readonly RecipeDetailViewModel _vm;

    public RecipeDetailPage(RecipeDetailViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
        // Une reconstruction (changement de portions) vide d'abord la liste : plus de cascade.
        Ingredients.ChildRemoved += (_, _) => _cascade &= !_opened; // le chargement peut reconstruire deux fois
        Ingredients.ChildAdded += (_, e) =>
        {
            if (_cascade && _opened && e.Element is VisualElement row) _ = row.RiseInAsync(Ingredients.Children.Count - 1);
        };
    }

    private bool _opened;
    private bool _cascade = true;

    // La mise en place : la photo se pose, le titre monte, les ingrédients suivent en cascade,
    // une seule fois. Au retour (après « Modifier », par exemple), la recette est relue, sans animation.
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (_opened)
        {
            _ = _vm.RefreshAsync();
            return;
        }
        _opened = true;
        if (Hero.IsVisible) _ = Hero.SettleInAsync(); // sans photo : rien à poser (et pas d'opacité 0 laissée)
        _ = TitleLabel.RiseInAsync();
        if (!_cascade) return;
        for (var i = 0; i < Ingredients.Children.Count; i++)
            if (Ingredients.Children[i] is VisualElement row) _ = row.RiseInAsync(i, MotionTokens.ItemStaggerMs * 2);
    }
}
