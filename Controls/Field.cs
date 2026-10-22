namespace Popote.Controls;

// Champ maison : libellé Eyebrow au-dessus, cadre arrondi, focus aubergine, suffixe
// (« min ») ou chevron (sélecteur), message d'aide. Le champ natif (Entry, Editor, Picker)
// est passé en contenu ; son soulignement Android est retiré par handler.
//   <controls:Field Label="Titre"><Entry Text="{Binding Title}" /></controls:Field>
[ContentProperty(nameof(Input))]
public class Field : ContentView
{
    public static readonly BindableProperty InputProperty = BindableProperty.Create(
        nameof(Input), typeof(View), typeof(Field), null, propertyChanged: (b, o, n) => ((Field)b).OnInputChanged((View?)o, (View?)n));

    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(Field), null, propertyChanged: (b, _, n) => ((Field)b).OnLabelChanged((string?)n));

    public static readonly BindableProperty SuffixProperty = BindableProperty.Create(
        nameof(Suffix), typeof(string), typeof(Field), null, propertyChanged: (b, _, n) => { var f = (Field)b; f._suffix.Text = (string?)n; f._suffix.IsVisible = n is string s && s.Length > 0; });

    public static readonly BindableProperty ShowChevronProperty = BindableProperty.Create(
        nameof(ShowChevron), typeof(bool), typeof(Field), false, propertyChanged: (b, _, n) => ((Field)b)._chevron.IsVisible = (bool)n);

    public static readonly BindableProperty ErrorProperty = BindableProperty.Create(
        nameof(Error), typeof(string), typeof(Field), null, propertyChanged: (b, _, _) => ((Field)b).UpdateError());

    public static readonly BindableProperty LeadingProperty = BindableProperty.Create(
        nameof(Leading), typeof(View), typeof(Field), null, propertyChanged: (b, _, n) => ((Field)b).SetLeading((View?)n));

    public View? Input { get => (View?)GetValue(InputProperty); set => SetValue(InputProperty, value); }
    public string? Label { get => (string?)GetValue(LabelProperty); set => SetValue(LabelProperty, value); }
    public string? Suffix { get => (string?)GetValue(SuffixProperty); set => SetValue(SuffixProperty, value); }
    public bool ShowChevron { get => (bool)GetValue(ShowChevronProperty); set => SetValue(ShowChevronProperty, value); }
    // Élément décoratif à gauche (icône de recherche, bol du rayon…)
    // Message d'erreur sous le champ (dit ce qui ne va pas et comment corriger) ; vide = aucun.
    public string? Error { get => (string?)GetValue(ErrorProperty); set => SetValue(ErrorProperty, value); }
    public View? Leading { get => (View?)GetValue(LeadingProperty); set => SetValue(LeadingProperty, value); }

    private readonly Label _label = new() { IsVisible = false };
    private readonly Label _suffix = new() { IsVisible = false, VerticalOptions = LayoutOptions.Center };
    private readonly Icon _chevron = new() { Glyph = "IconChevron", Size = 18, Role = "Mute", IsVisible = false };
    private readonly Label _error = new() { IsVisible = false };
    private readonly Grid _row;
    private readonly Border _frame;

    public Field()
    {
        _label.Style = Tokens.Style("Eyebrow");
        _suffix.Style = Tokens.Style("Caption");
        _error.Style = Tokens.Style("Caption");
        _error.Themed(Microsoft.Maui.Controls.Label.TextColorProperty, "Danger");
        AutomationProperties.SetIsInAccessibleTree(_label, false); // le libellé est porté par le champ
        _row = new Grid
        {
            ColumnDefinitions = { new(GridLength.Auto), new(GridLength.Star), new(GridLength.Auto), new(GridLength.Auto) },
            ColumnSpacing = 8,
        };
        _row.Add(_suffix, 2);
        _row.Add(_chevron, 3);
        _frame = new Border { Style = Tokens.Style("FieldFrame"), Content = _row };
        _frame.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => Input?.Focus()) });
        Content = new VerticalStackLayout { Spacing = Tokens.Size("Space4") + 2, Children = { _label, _frame, _error } };
        SetFocused(false);
    }

    private void OnLabelChanged(string? text)
    {
        _label.Text = text;
        _label.IsVisible = !string.IsNullOrEmpty(text);
        if (Input is not null) SemanticProperties.SetDescription(Input, text);
    }

    private void SetLeading(View? view)
    {
        if (view is null) return;
        view.VerticalOptions = LayoutOptions.Center;
        view.InputTransparent = true;
        _row.Add(view, 0);
    }

    private void OnInputChanged(View? old, View? input)
    {
        if (old is not null)
        {
            old.Focused -= OnFocused;
            old.Unfocused -= OnUnfocused;
            _row.Remove(old);
        }
        if (input is null) return;
        input.VerticalOptions = LayoutOptions.Center;
        input.Focused += OnFocused;
        input.Unfocused += OnUnfocused;
        if (Label is not null) SemanticProperties.SetDescription(input, Label);
        _row.Add(input, 1);
    }

    private void UpdateError()
    {
        _error.Text = Error;
        _error.IsVisible = !string.IsNullOrEmpty(Error);
        SetFocused(Input?.IsFocused == true);
        if (!string.IsNullOrEmpty(Error)) SemanticScreenReader.Default.Announce(Error);
    }

    private void OnFocused(object? s, FocusEventArgs e) => SetFocused(true);
    private void OnUnfocused(object? s, FocusEventArgs e) => SetFocused(false);

    private void SetFocused(bool focused)
    {
        var error = !string.IsNullOrEmpty(Error);
        _frame.Themed(Border.StrokeProperty, error ? "Danger" : focused ? "Accent" : "Line");
        _frame.StrokeThickness = focused || error ? 2 : Tokens.Size("Hairline");
    }
}
