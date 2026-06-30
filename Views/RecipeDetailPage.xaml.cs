using Popote.ViewModels;

namespace Popote.Views;

public partial class RecipeDetailPage : ContentPage
{
    public RecipeDetailPage(RecipeDetailViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
