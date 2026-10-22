namespace Popote.Controls;

// Case à cocher maison de la liste de courses : un bol qui se remplit quand l'article est pris.
// Cible de 48 ; TalkBack l'annonce comme « case à cocher » avec son état (A11y).
// Description = phrase complète portée par la ligne (ex. « Lait, 50 centilitres »).
public class BowlCheck : ContentView, IActivatable
{
    public static readonly BindableProperty IsCheckedProperty = BindableProperty.Create(
        nameof(IsChecked), typeof(bool), typeof(BowlCheck), false, BindingMode.TwoWay,
        propertyChanged: (b, _, _) => ((BowlCheck)b).Update());

    public static readonly BindableProperty AisleProperty = BindableProperty.Create(
        nameof(Aisle), typeof(string), typeof(BowlCheck), null,
        propertyChanged: (b, _, n) => ((BowlCheck)b)._bowl.Aisle = (string?)n);

    public static readonly BindableProperty UseAccentProperty = BindableProperty.Create(
        nameof(UseAccent), typeof(bool), typeof(BowlCheck), false,
        propertyChanged: (b, _, n) => ((BowlCheck)b)._bowl.UseAccent = (bool)n);

    public static readonly BindableProperty DescriptionProperty = BindableProperty.Create(
        nameof(Description), typeof(string), typeof(BowlCheck), null,
        propertyChanged: (b, _, n) => SemanticProperties.SetDescription(b, (string?)n));

    public bool IsChecked { get => (bool)GetValue(IsCheckedProperty); set => SetValue(IsCheckedProperty, value); }
    public string? Aisle { get => (string?)GetValue(AisleProperty); set => SetValue(AisleProperty, value); }
    // Émail aubergine plutôt que la couleur d'un rayon (ex. choix de recettes).
    public bool UseAccent { get => (bool)GetValue(UseAccentProperty); set => SetValue(UseAccentProperty, value); }
    public string? Description { get => (string?)GetValue(DescriptionProperty); set => SetValue(DescriptionProperty, value); }

    private readonly Bowl _bowl;
    private readonly Icon _check;

    public event EventHandler? Toggled;

    public BowlCheck()
    {
        var size = Tokens.Size("BowlCheck");
        WidthRequest = HeightRequest = Tokens.Size("TouchTarget");
        _bowl = new Bowl { Size = size, State = BowlState.Empty };
        _check = new Icon { Glyph = "IconCheck", Size = size * 0.5, Role = "Background", StrokeThickness = 3 };
        var grid = new Grid { InputTransparent = true, Children = { _bowl, _check } };
        AutomationProperties.SetIsInAccessibleTree(grid, false);
        Content = grid;
        A11y.SetRole(this, A11yRole.CheckBox);
        GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(Activate) });
        Update();
    }

    public Bowl Bowl => _bowl;

    public void Activate()
    {
        IsChecked = !IsChecked;
        if (IsChecked) _ = _bowl.FillAsync(); // seulement au geste : jamais au recyclage d'une cellule
        Motion.Haptics.Tick();
        Toggled?.Invoke(this, EventArgs.Empty);
    }

    private void Update()
    {
        _bowl.StopFill();
        _bowl.State = IsChecked ? BowlState.Full : BowlState.Empty;
        _bowl.Size = Tokens.Size("BowlCheck");
        _check.IsVisible = IsChecked;
        A11y.SetIsChecked(this, IsChecked);
    }
}
