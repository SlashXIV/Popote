using Popote.ViewModels;

namespace Popote.Views;

public partial class CookWithPage : ContentPage
{
    private readonly CookWithViewModel _vm;

    public CookWithPage(CookWithViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadCommand.ExecuteAsync(null);
    }
}
