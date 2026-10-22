using Popote.Motion;
using Popote.ViewModels;

namespace Popote.Views;

public partial class RecipeEditPage : ContentPage
{
    public RecipeEditPage(RecipeEditViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(RecipeEditViewModel.IsSaved) && vm.IsSaved)
                _ = SavedBadge.RiseInAsync();
        };
    }
}
