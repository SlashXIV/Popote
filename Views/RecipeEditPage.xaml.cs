using Popote.ViewModels;

namespace Popote.Views;

public partial class RecipeEditPage : ContentPage
{
    public RecipeEditPage(RecipeEditViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
