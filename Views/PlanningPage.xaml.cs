using Popote.ViewModels;

namespace Popote.Views;

public partial class PlanningPage : ContentPage
{
    private readonly PlanningViewModel _vm;

    public PlanningPage(PlanningViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    // Recharge la semaine à chaque affichage (repas ajoutés ailleurs, etc.).
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadCommand.ExecuteAsync(null);
    }
}
