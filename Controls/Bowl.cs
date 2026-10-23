using Popote.Motion;

namespace Popote.Controls;

public enum BowlState { Empty, Placed, Full }

// Le bol vu de dessus, unité visuelle de Popote : un bord émaillé, un creux, un contenu.
// Émail = couleur du rayon (Aisle) ou aubergine (Accent). Décoratif : le libellé du rayon
// ou de l'ingrédient porte toujours l'information à côté.
// Une seule vue dessinée (GraphicsView) : les listes en affichent des dizaines (mesuré : des
// formes MAUI empilées coûtaient des images au défilement).
public class Bowl : GraphicsView, IDrawable
{
    public static readonly BindableProperty AisleProperty = BindableProperty.Create(
        nameof(Aisle), typeof(string), typeof(Bowl), null, propertyChanged: (b, _, _) => ((Bowl)b).UpdateGlaze());

    public static readonly BindableProperty UseAccentProperty = BindableProperty.Create(
        nameof(UseAccent), typeof(bool), typeof(Bowl), false, propertyChanged: (b, _, _) => ((Bowl)b).UpdateGlaze());

    public static readonly BindableProperty StateProperty = BindableProperty.Create(
        nameof(State), typeof(BowlState), typeof(Bowl), BowlState.Placed, propertyChanged: (b, o, n) => ((Bowl)b).OnStateChanged((BowlState)o, (BowlState)n));

    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(double), typeof(Bowl), 18.0, propertyChanged: (b, _, n) => ((Bowl)b).WidthRequest = ((Bowl)b).HeightRequest = (double)n);

    // Couleur effective (suit le thème via AppThemeBinding) ; redessine à chaque changement.
    public static readonly BindableProperty GlazeProperty = BindableProperty.Create(
        nameof(Glaze), typeof(Color), typeof(Bowl), Colors.Gray, propertyChanged: (b, _, _) => ((Bowl)b).Invalidate());

    // Part du contenu (0..1), animable par le jalon Moments (« le bol se remplit »).
    public static readonly BindableProperty FillProgressProperty = BindableProperty.Create(
        nameof(FillProgress), typeof(double), typeof(Bowl), 1.0, propertyChanged: (b, _, _) => ((Bowl)b).Invalidate());

    public string? Aisle { get => (string?)GetValue(AisleProperty); set => SetValue(AisleProperty, value); }
    public bool UseAccent { get => (bool)GetValue(UseAccentProperty); set => SetValue(UseAccentProperty, value); }
    public BowlState State { get => (BowlState)GetValue(StateProperty); set => SetValue(StateProperty, value); }
    public double Size { get => (double)GetValue(SizeProperty); set => SetValue(SizeProperty, value); }
    public Color Glaze { get => (Color)GetValue(GlazeProperty); set => SetValue(GlazeProperty, value); }
    public double FillProgress { get => (double)GetValue(FillProgressProperty); set => SetValue(FillProgressProperty, value); }

    // Rejoue le remplissage quand l'état passe à Full (bols de progression du mode cuisson).
    public bool AnimatesFill { get; set; }

    public Bowl()
    {
        Drawable = this;
        BackgroundColor = Colors.Transparent;
        InputTransparent = true;
        HorizontalOptions = LayoutOptions.Center;
        VerticalOptions = LayoutOptions.Center;
        WidthRequest = HeightRequest = Size;
        AutomationProperties.SetIsInAccessibleTree(this, false);
        UpdateGlaze();
    }

    private const string FillAnimation = "Fill";

    // Le bol se remplit (« article coché », « étape faite ») selon le niveau de mouvement :
    // rebond Release, fondu court en Sobre, plein d'emblée si les animations sont coupées.
    public Task FillAsync(double from = 0)
    {
        StopFill();
        if (MotionTokens.Level == MotionLevel.None) return Task.CompletedTask;
        var full = MotionTokens.Level == MotionLevel.Full;
        var done = new TaskCompletionSource();
        new Animation(v => FillProgress = v, from, 1, full ? MotionTokens.Release : Easing.Linear)
            .Commit(this, FillAnimation, length: full ? MotionTokens.ReleaseMs : MotionTokens.ReducedMs,
                finished: (_, _) => { FillProgress = 1; done.TrySetResult(); });
        return done.Task;
    }

    // Coupe un remplissage en cours (cellule recyclée, état changé ailleurs) : bol plein.
    public void StopFill()
    {
        this.AbortAnimation(FillAnimation);
        FillProgress = 1;
    }

    private void OnStateChanged(BowlState old, BowlState state)
    {
        if (AnimatesFill && state == BowlState.Full && old != BowlState.Full)
            _ = FillAsync(Ratio(old) / Ratio(BowlState.Full)); // repart du contenu déjà posé
        else
            Invalidate();
    }

    private static double Ratio(BowlState state) => state switch { BowlState.Full => 0.64, BowlState.Placed => 0.36, _ => 0 };

    private void UpdateGlaze()
    {
        if (Application.Current is null) return;
        this.Themed(GlazeProperty, UseAccent ? "Accent" : AislePalette.RoleFor(Aisle));
    }

    public void Draw(ICanvas canvas, RectF rect)
    {
        var size = Math.Min(rect.Width, rect.Height);
        var center = rect.Center;
        var rim = (float)Math.Max(1.5, size * 0.12);
        canvas.StrokeColor = Glaze;
        canvas.StrokeSize = rim;
        canvas.DrawCircle(center, (size - rim) / 2);

        var inner = Ratio(State) * FillProgress;
        if (inner <= 0) return;
        canvas.FillColor = Glaze;
        canvas.FillCircle(center, (float)(size * inner / 2));
    }
}

// Correspondance rayon (donnée de Ingredient.Aisle, saisie libre) → rôle de couleur.
public static class AislePalette
{
    public static string RoleFor(string? aisle) => (aisle ?? string.Empty).Trim().ToLowerInvariant() switch
    {
        "fruits & légumes" or "fruits et légumes" or "légumes" or "fruits" => "AisleProduce",
        "crèmerie" or "cremerie" or "frais" or "produits frais" => "AisleDairy",
        "épicerie" or "epicerie" => "AislePantry",
        "viande & poisson" or "viande et poisson" or "viande" or "poisson" or "boucherie" => "AisleMeat",
        "surgelés" or "surgeles" => "AisleFrozen",
        _ => "AisleOther",
    };
}
