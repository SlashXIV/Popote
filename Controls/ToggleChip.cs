using System.Windows.Input;

namespace Popote.Controls;

// Chip à bascule (filtres de tags, tags d'une recette, « Cuisiner avec… »).
// Ne rétrécit jamais son texte ; sélection = fond encre + coche ; TalkBack : « bouton bascule ».
public class ToggleChip : ContentView, IActivatable
{
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(ToggleChip), string.Empty,
        propertyChanged: (b, _, n) => { var c = (ToggleChip)b; c._label.Text = (string)n; c.UpdateDescription(); });

    public static readonly BindableProperty IsSelectedProperty = BindableProperty.Create(
        nameof(IsSelected), typeof(bool), typeof(ToggleChip), false, propertyChanged: (b, _, _) => ((ToggleChip)b).Update());

    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command), typeof(ICommand), typeof(ToggleChip));

    public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
        nameof(CommandParameter), typeof(object), typeof(ToggleChip));

    public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }
    public bool IsSelected { get => (bool)GetValue(IsSelectedProperty); set => SetValue(IsSelectedProperty, value); }
    public ICommand? Command { get => (ICommand?)GetValue(CommandProperty); set => SetValue(CommandProperty, value); }
    public object? CommandParameter { get => GetValue(CommandParameterProperty); set => SetValue(CommandParameterProperty, value); }

    private readonly Border _pill;
    private readonly Label _label;
    private readonly Icon _check = new() { Glyph = "IconCheck", Size = 16, Role = "Background", StrokeThickness = 2.4 };

    public ToggleChip()
    {
        _label = new Label { Style = Tokens.Style("ChipLabel") };
        var row = new HorizontalStackLayout { Spacing = 6, Children = { _check, _label }, InputTransparent = true };
        _pill = new Border { Style = Tokens.Style("Chip"), Content = row, InputTransparent = true, VerticalOptions = LayoutOptions.Center };
        AutomationProperties.SetIsInAccessibleTree(_pill, false);
        Content = _pill;
        MinimumHeightRequest = Tokens.Size("TouchTarget");
        A11y.SetRole(this, A11yRole.Switch);
        GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(Activate) });
        Update();
    }

    public void Activate()
    {
        Motion.Haptics.Tick();
        if (Command?.CanExecute(CommandParameter) == true) Command.Execute(CommandParameter);
    }

    private void Update()
    {
        _check.IsVisible = IsSelected;
        if (IsSelected)
        {
            _pill.Themed(BackgroundColorProperty, "Ink");
            _pill.Themed(Border.StrokeProperty, "Ink");
            _label.Themed(Label.TextColorProperty, "Background");
        }
        else
        {
            _pill.BackgroundColor = Colors.Transparent;
            _pill.Themed(Border.StrokeProperty, "Line");
            _label.Themed(Label.TextColorProperty, "Ink");
        }
        A11y.SetIsChecked(this, IsSelected);
    }

    private void UpdateDescription() => SemanticProperties.SetDescription(this, Text);
}
