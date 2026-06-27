using RecettesApp.ViewModels;

namespace RecettesApp.Views;

public partial class RecipeEditPage : ContentPage
{
    public RecipeEditPage(RecipeEditViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
