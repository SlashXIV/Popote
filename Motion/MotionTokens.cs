namespace Popote.Motion;

// Système de mouvement centralisé (docs/design-system.md, section Mouvement) : durées,
// trois courbes, niveau courant. Toute animation de l'app passe par ici ; elle est
// déclenchée par un événement, jamais en boucle, et annulable (CancelAnimations).
public static class MotionTokens
{
    // Durées (ms)
    public const uint EnterMs = 420;        // éléments qui se posent
    public const uint TransitionMs = 480;   // changements d'état ou de page
    public const uint ReleaseMs = 560;      // bols qui se remplissent (léger rebond)
    public const uint ReducedMs = 150;      // Sobre : fondu court
    public const uint PressMs = 90;
    public const uint ItemStaggerMs = 50;   // décalage entre éléments d'une liste
    public const int MaxStaggered = 8;      // au-delà, plus de décalage
    public const uint LetterStaggerMs = 40;
    public const double RiseDistance = 12;
    public const double PressScale = 0.97;
    public const double OverflowScale = 1.25;   // minuteur fini : le bol déborde…
    public const int OverflowCount = 3;         // …trois fois, puis s'arrête

    // Courbes (mêmes valeurs que la planche)
    public static readonly Easing Enter = Make(0.2, 0.7, 0.2, 1);
    public static readonly Easing Transition = Make(0.65, 0, 0.35, 1);
    public static readonly Easing Release = Make(0.2, 0.9, 0.2, 1.2);

    private static Easing Make(double x1, double y1, double x2, double y2)
    {
        var curve = new CubicBezier(x1, y1, x2, y2);
        return new Easing(curve.Evaluate);
    }

    // Ambiance « Sobre » (réglage de l'app), poussée par SettingsService.
    public static bool Sober { get; set; }

    // Échelle d'animation du système, relue au démarrage et à chaque retour au premier plan.
    public static float SystemScale { get; private set; } = 1f;

    public static MotionLevel Level => MotionLevels.Of(SystemScale, Sober);

    public static void RefreshSystemScale()
    {
#if ANDROID
        var resolver = Android.App.Application.Context.ContentResolver;
        SystemScale = Android.Provider.Settings.Global.GetFloat(
            resolver, Android.Provider.Settings.Global.AnimatorDurationScale, 1f);
#endif
    }
}
