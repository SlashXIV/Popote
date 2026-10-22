using System.Collections;

namespace Popote.Controls;

// Sélecteur segmenté (choix exclusif court : thème, Ambiance, multiplicateur de portions).
// Chaque segment est une cible de 48 annoncée « case d'option, cochée / non cochée ».
public class Segmented : ContentView
{
    public static readonly BindableProperty ItemsProperty = BindableProperty.Create(
        nameof(Items), typeof(IList), typeof(Segmented), null, propertyChanged: (b, _, _) => ((Segmented)b).Build());

    public static readonly BindableProperty SelectedIndexProperty = BindableProperty.Create(
        nameof(SelectedIndex), typeof(int), typeof(Segmented), -1, BindingMode.TwoWay, propertyChanged: (b, _, _) => ((Segmented)b).Update());

    public IList? Items { get => (IList?)GetValue(ItemsProperty); set => SetValue(ItemsProperty, value); }
    public int SelectedIndex { get => (int)GetValue(SelectedIndexProperty); set => SetValue(SelectedIndexProperty, value); }

    private readonly Grid _grid = new() { Padding = 4, ColumnSpacing = 4 };
    private readonly List<Segment> _segments = new();

    public Segmented()
    {
        var frame = new Border { StrokeThickness = 0, StrokeShape = Tokens.Rounded("RadiusCard"), Content = _grid };
        frame.Themed(BackgroundColorProperty, "Worktop");
        Content = frame;
    }

    private void Build()
    {
        _grid.Children.Clear();
        _grid.ColumnDefinitions.Clear();
        _segments.Clear();
        if (Items is null) return;
        for (var i = 0; i < Items.Count; i++)
        {
            var index = i;
            var seg = new Segment(Items[i]?.ToString() ?? string.Empty, () => { SelectedIndex = index; Motion.Haptics.Tick(); });
            _grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            _grid.Add(seg, i);
            _segments.Add(seg);
        }
        Update();
    }

    private void Update()
    {
        for (var i = 0; i < _segments.Count; i++) _segments[i].SetSelected(i == SelectedIndex);
    }

    private sealed class Segment : ContentView, IActivatable
    {
        private readonly Action _select;
        private readonly Border _pill;
        private readonly Label _label;

        public Segment(string text, Action select)
        {
            _select = select;
            _label = new Label { Text = text, Style = Tokens.Style("ChipLabel"), HorizontalOptions = LayoutOptions.Center, FontFamily = "InterSemiBold" };
            _pill = new Border { StrokeThickness = 0, StrokeShape = Tokens.Rounded("RadiusField"), Content = _label, InputTransparent = true };
            AutomationProperties.SetIsInAccessibleTree(_pill, false);
            Content = _pill;
            MinimumHeightRequest = Tokens.Size("TouchTarget") - 8; // + 2 × 4 de marge = 48
            SemanticProperties.SetDescription(this, text);
            A11y.SetRole(this, A11yRole.Radio);
            GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(Activate) });
        }

        public void Activate() => _select();

        public void SetSelected(bool selected)
        {
            if (selected) _pill.Themed(BackgroundColorProperty, "Surface");
            else _pill.BackgroundColor = Colors.Transparent;
            _label.Themed(Label.TextColorProperty, selected ? "Ink" : "Mute");
            A11y.SetIsChecked(this, selected);
        }
    }
}
