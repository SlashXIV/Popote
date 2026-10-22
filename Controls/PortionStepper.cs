using Popote.Resources.Strings;

namespace Popote.Controls;

// Stepper de portions maison : −, nombre en chiffres tabulaires, +. Cibles de 48.
// TalkBack lit un seul nœud « 4 portions » ; chaque bouton a sa propre description.
public class PortionStepper : ContentView
{
    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value), typeof(int), typeof(PortionStepper), 1, BindingMode.TwoWay,
        propertyChanged: (b, _, _) => ((PortionStepper)b).Update());

    public static readonly BindableProperty MinimumProperty = BindableProperty.Create(
        nameof(Minimum), typeof(int), typeof(PortionStepper), 1, propertyChanged: (b, _, _) => ((PortionStepper)b).Update());

    public static readonly BindableProperty MaximumProperty = BindableProperty.Create(
        nameof(Maximum), typeof(int), typeof(PortionStepper), 50, propertyChanged: (b, _, _) => ((PortionStepper)b).Update());

    public static readonly BindableProperty CaptionProperty = BindableProperty.Create(
        nameof(Caption), typeof(string), typeof(PortionStepper), null, propertyChanged: (b, _, _) => ((PortionStepper)b).Update());

    public int Value { get => (int)GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    public int Minimum { get => (int)GetValue(MinimumProperty); set => SetValue(MinimumProperty, value); }
    public int Maximum { get => (int)GetValue(MaximumProperty); set => SetValue(MaximumProperty, value); }
    // Ligne d'aide sous le nombre (ex. « recette de base : 4 »).
    public string? Caption { get => (string?)GetValue(CaptionProperty); set => SetValue(CaptionProperty, value); }

    private readonly NumberLabel _number = new() { HorizontalTextAlignment = TextAlignment.Center };
    private readonly Label _caption = new() { HorizontalTextAlignment = TextAlignment.Center };
    private readonly VerticalStackLayout _center;
    private readonly IconButton _minus, _plus;

    public PortionStepper()
    {
        _number.Style = Tokens.Style("Counter");
        _caption.Style = Tokens.Style("Caption");
        _minus = new IconButton { Glyph = "IconMinus", Look = IconButtonLook.Tonal, Description = AppResources.Portions_Remove };
        _plus = new IconButton { Glyph = "IconPlus", Look = IconButtonLook.Tonal, Description = AppResources.Portions_Add };
        _minus.Clicked += (_, _) => Step(-1);
        _plus.Clicked += (_, _) => Step(+1);

        _center = new VerticalStackLayout { VerticalOptions = LayoutOptions.Center, Children = { _number, _caption } };
        AutomationProperties.SetIsInAccessibleTree(_number, false);
        AutomationProperties.SetIsInAccessibleTree(_caption, false);

        var grid = new Grid
        {
            ColumnDefinitions = { new(GridLength.Auto), new(GridLength.Star), new(GridLength.Auto) },
            Padding = 4,
        };
        grid.Add(_minus, 0);
        grid.Add(_center, 1);
        grid.Add(_plus, 2);
        Content = new Border { Style = Tokens.Style("Card"), Padding = 0, Content = grid };
        Update();
    }

    private void Step(int delta)
    {
        var next = Math.Clamp(Value + delta, Minimum, Maximum);
        if (next == Value) return;
        Value = next;
        Motion.Haptics.Tick();
        SemanticScreenReader.Default.Announce(PortionsText(next));
    }

    private static string PortionsText(int n) => string.Format(n > 1 ? AppResources.Portions_Many : AppResources.Portions_One, n);

    private void Update()
    {
        _number.Text = Value.ToString();
        _caption.Text = Caption ?? (Value > 1 ? AppResources.Portions_Word_Many : AppResources.Portions_Word_One);
        SemanticProperties.SetDescription(_center, PortionsText(Value) + (Caption is null ? string.Empty : ", " + Caption));
        _minus.Opacity = Value > Minimum ? 1 : 0.4;
        _plus.Opacity = Value < Maximum ? 1 : 0.4;
    }
}
