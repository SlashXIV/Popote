namespace Popote.Controls;

// Interrupteur maison (le Switch Material rendait une pastille tronquée) : piste aubergine
// quand activé, pastille crème. Cible de 48 ; TalkBack : « bouton bascule, activé ».
public class ToggleSwitch : ContentView, IActivatable
{
    public static readonly BindableProperty IsToggledProperty = BindableProperty.Create(
        nameof(IsToggled), typeof(bool), typeof(ToggleSwitch), false, BindingMode.TwoWay,
        propertyChanged: (b, _, _) => ((ToggleSwitch)b).Update());

    public static readonly BindableProperty DescriptionProperty = BindableProperty.Create(
        nameof(Description), typeof(string), typeof(ToggleSwitch), null,
        propertyChanged: (b, _, n) => SemanticProperties.SetDescription(b, (string?)n));

    public bool IsToggled { get => (bool)GetValue(IsToggledProperty); set => SetValue(IsToggledProperty, value); }
    public string? Description { get => (string?)GetValue(DescriptionProperty); set => SetValue(DescriptionProperty, value); }

    private const double TrackWidth = 46, TrackHeight = 28, Knob = 22;
    private readonly Border _track;
    private readonly Microsoft.Maui.Controls.Shapes.Ellipse _knob = new() { WidthRequest = Knob, HeightRequest = Knob, VerticalOptions = LayoutOptions.Center };

    public ToggleSwitch()
    {
        _track = new Border
        {
            WidthRequest = TrackWidth, HeightRequest = TrackHeight, StrokeThickness = 0, Padding = 3,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = TrackHeight / 2 },
            VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center,
            Content = _knob, InputTransparent = true,
        };
        AutomationProperties.SetIsInAccessibleTree(_track, false);
        WidthRequest = HeightRequest = Tokens.Size("TouchTarget");
        Content = _track;
        A11y.SetRole(this, A11yRole.Switch);
        GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(Activate) });
        Update();
    }

    public void Activate()
    {
        IsToggled = !IsToggled;
        Motion.Haptics.Tick();
    }

    private void Update()
    {
        _track.Themed(BackgroundColorProperty, IsToggled ? "Accent" : "Line");
        _knob.Themed(Microsoft.Maui.Controls.Shapes.Shape.FillProperty, IsToggled ? "OnAccent" : "Surface");
        _knob.HorizontalOptions = IsToggled ? LayoutOptions.End : LayoutOptions.Start;
        A11y.SetIsChecked(this, IsToggled);
    }
}
