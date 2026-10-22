using System.Windows.Input;

namespace Popote.Controls;

public enum IconButtonLook { Plain, Tonal, OnPhoto }

// Bouton icône : cible de 48, une icône au centre, une description obligatoire pour TalkBack
// (exposé comme « bouton » via A11y.Role). Remplace les ToolbarItems et les « ✕ » en texte.
public class IconButton : ContentView, IActivatable
{
    public static readonly BindableProperty GlyphProperty = BindableProperty.Create(
        nameof(Glyph), typeof(string), typeof(IconButton), null, propertyChanged: (b, _, n) => ((IconButton)b)._icon.Glyph = (string?)n);

    public static readonly BindableProperty FilledProperty = BindableProperty.Create(
        nameof(Filled), typeof(bool), typeof(IconButton), false, propertyChanged: (b, _, n) => ((IconButton)b)._icon.Filled = (bool)n);

    public static readonly BindableProperty IconRoleProperty = BindableProperty.Create(
        nameof(IconRole), typeof(string), typeof(IconButton), "Ink", propertyChanged: (b, _, n) => ((IconButton)b)._icon.Role = (string)n);

    public static readonly BindableProperty LookProperty = BindableProperty.Create(
        nameof(Look), typeof(IconButtonLook), typeof(IconButton), IconButtonLook.Plain, propertyChanged: (b, _, _) => ((IconButton)b).ApplyLook());

    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command), typeof(ICommand), typeof(IconButton));

    public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
        nameof(CommandParameter), typeof(object), typeof(IconButton));

    public static readonly BindableProperty DescriptionProperty = BindableProperty.Create(
        nameof(Description), typeof(string), typeof(IconButton), null,
        propertyChanged: (b, _, n) => SemanticProperties.SetDescription(b, (string?)n));

    public string? Glyph { get => (string?)GetValue(GlyphProperty); set => SetValue(GlyphProperty, value); }
    public bool Filled { get => (bool)GetValue(FilledProperty); set => SetValue(FilledProperty, value); }
    public string IconRole { get => (string)GetValue(IconRoleProperty); set => SetValue(IconRoleProperty, value); }
    public IconButtonLook Look { get => (IconButtonLook)GetValue(LookProperty); set => SetValue(LookProperty, value); }
    public ICommand? Command { get => (ICommand?)GetValue(CommandProperty); set => SetValue(CommandProperty, value); }
    public object? CommandParameter { get => GetValue(CommandParameterProperty); set => SetValue(CommandParameterProperty, value); }
    public string? Description { get => (string?)GetValue(DescriptionProperty); set => SetValue(DescriptionProperty, value); }

    private readonly Icon _icon = new();
    private readonly Border _disc;

    public event EventHandler? Clicked;

    public IconButton()
    {
        var target = Tokens.Size("TouchTarget");
        WidthRequest = HeightRequest = target;
        _disc = new Border
        {
            StrokeThickness = 0,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.Ellipse(),
            WidthRequest = target - 4,
            HeightRequest = target - 4,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            BackgroundColor = Colors.Transparent,
            InputTransparent = true,
            Content = _icon,
        };
        AutomationProperties.SetIsInAccessibleTree(_disc, false);
        Content = _disc;
        A11y.SetRole(this, A11yRole.Button);
        GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(Activate) });
    }

    public void Activate()
    {
        _ = Motion.Entrances.PressAsync(_disc);
        Clicked?.Invoke(this, EventArgs.Empty);
        if (Command?.CanExecute(CommandParameter) == true)
            Command.Execute(CommandParameter);
    }

    private void ApplyLook()
    {
        switch (Look)
        {
            case IconButtonLook.Tonal: _disc.Themed(BackgroundColorProperty, "Worktop"); break;
            case IconButtonLook.OnPhoto: _disc.Themed(BackgroundColorProperty, "Scrim"); break;
            default: _disc.BackgroundColor = Colors.Transparent; break;
        }
    }
}
