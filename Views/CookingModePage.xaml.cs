using Popote.ViewModels;

namespace Popote.Views;

public partial class CookingModePage : ContentPage
{
    private readonly CookingModeViewModel _vm;

    public CookingModePage(CookingModeViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    // Garde l'écran allumé pendant la cuisson.
    protected override void OnAppearing()
    {
        base.OnAppearing();
        DeviceDisplay.Current.KeepScreenOn = true;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        DeviceDisplay.Current.KeepScreenOn = false;
        _vm.StopTimerCommand.Execute(null); // arrête le minuteur en quittant
    }
}
