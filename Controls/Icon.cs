namespace Popote.Controls;

// Icône au trait sur grille 24 (données de chemin SVG dans Tokens.xaml, clés « Icon… »),
// teinte par rôle de couleur (suit le thème). Une seule vue dessinée (GraphicsView).
// Décorative : hors de l'arbre d'accessibilité (c'est le bouton qui porte la description).
public class Icon : GraphicsView, IDrawable
{
    public static readonly BindableProperty GlyphProperty = BindableProperty.Create(
        nameof(Glyph), typeof(string), typeof(Icon), null, propertyChanged: (b, _, _) => ((Icon)b).UpdatePath());

    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(double), typeof(Icon), 22.0, propertyChanged: (b, _, n) => ((Icon)b).WidthRequest = ((Icon)b).HeightRequest = (double)n);

    public static readonly BindableProperty FilledProperty = BindableProperty.Create(
        nameof(Filled), typeof(bool), typeof(Icon), false, propertyChanged: (b, _, _) => ((Icon)b).Invalidate());

    public static readonly BindableProperty RoleProperty = BindableProperty.Create(
        nameof(Role), typeof(string), typeof(Icon), "Ink", propertyChanged: (b, _, _) => ((Icon)b).UpdateTint());

    public static readonly BindableProperty StrokeThicknessProperty = BindableProperty.Create(
        nameof(StrokeThickness), typeof(double), typeof(Icon), 1.9, propertyChanged: (b, _, _) => ((Icon)b).Invalidate());

    public static readonly BindableProperty TintProperty = BindableProperty.Create(
        nameof(Tint), typeof(Color), typeof(Icon), Colors.Gray, propertyChanged: (b, _, _) => ((Icon)b).Invalidate());

    // Clé de ressource (ex. « IconBack ») ou données de chemin brutes.
    public string? Glyph { get => (string?)GetValue(GlyphProperty); set => SetValue(GlyphProperty, value); }
    public double Size { get => (double)GetValue(SizeProperty); set => SetValue(SizeProperty, value); }
    public bool Filled { get => (bool)GetValue(FilledProperty); set => SetValue(FilledProperty, value); }
    // Rôle de couleur (Ink, Mute, Accent, Favorite, Background…)
    public string Role { get => (string)GetValue(RoleProperty); set => SetValue(RoleProperty, value); }
    public double StrokeThickness { get => (double)GetValue(StrokeThicknessProperty); set => SetValue(StrokeThicknessProperty, value); }
    public Color Tint { get => (Color)GetValue(TintProperty); set => SetValue(TintProperty, value); }

    private PathF? _path;

    public Icon()
    {
        Drawable = this;
        BackgroundColor = Colors.Transparent;
        InputTransparent = true;
        HorizontalOptions = LayoutOptions.Center;
        VerticalOptions = LayoutOptions.Center;
        WidthRequest = HeightRequest = Size;
        AutomationProperties.SetIsInAccessibleTree(this, false);
        UpdateTint();
    }

    private void UpdatePath()
    {
        var data = Glyph is null ? null
            : Application.Current?.Resources.TryGetValue(Glyph, out var v) == true ? (string)v : Glyph;
        _path = data is null ? null : PathBuilder.Build(data);
        Invalidate();
    }

    private void UpdateTint()
    {
        if (Application.Current is null) return;
        this.Themed(TintProperty, Role);
    }

    public void Draw(ICanvas canvas, RectF rect)
    {
        if (_path is null) return;
        var scale = Math.Min(rect.Width, rect.Height) / 24f; // grille 24 -> taille demandée
        canvas.SaveState();
        canvas.Scale(scale, scale);
        canvas.StrokeColor = Tint;
        canvas.StrokeSize = (float)StrokeThickness;
        canvas.StrokeLineCap = LineCap.Round;
        canvas.StrokeLineJoin = LineJoin.Round;
        if (Filled)
        {
            canvas.FillColor = Tint;
            canvas.FillPath(_path);
        }
        canvas.DrawPath(_path);
        canvas.RestoreState();
    }
}
