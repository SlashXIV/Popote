using Popote.Motion;
using Popote.ViewModels;

namespace Popote.Views;

public partial class CookingModePage : ContentPage
{
    private readonly CookingModeViewModel _vm;

    public CookingModePage(CookingModeViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
        vm.TimerFinished += (_, _) => _ = TimerBowl.OverflowAsync();
        // Passage au poste : les bols de progression se posent en cascade à leur création.
        ProgressRow.ChildAdded += (_, e) =>
        {
            if (e.Element is VisualElement bowl) _ = bowl.RiseInAsync(ProgressRow.Children.Count - 1);
        };
    }

    private bool _entered;

    // Garde l'écran allumé pendant la cuisson.
    protected override void OnAppearing()
    {
        base.OnAppearing();
        DeviceDisplay.Current.KeepScreenOn = true;
        if (!_entered) Haptics.Confirm(); // entrée en cuisson, une seule fois
        _entered = true;
        StepsCarousel.IsScrollAnimated = MotionTokens.Level != MotionLevel.None; // « Supprimer les animations »
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        DeviceDisplay.Current.KeepScreenOn = false;
        TimerBowl.CancelAnimations();
        _vm.StopTimerCommand.Execute(null); // arrête le minuteur en quittant
    }
}
