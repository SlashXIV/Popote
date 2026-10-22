using AndroidX.Core.View;

namespace Popote;

// Icônes des barres système (heure, batterie, navigation) claires sur thème sombre,
// sombres sur thème clair. Les fonds restent ceux de l'app (bord à bord).
public static class SystemBars
{
    public static void Apply(bool dark)
    {
        var window = Platform.CurrentActivity?.Window;
        if (window?.DecorView is null) return;
        var controller = WindowCompat.GetInsetsController(window, window.DecorView);
        if (controller is null) return;
        controller.AppearanceLightStatusBars = !dark;
        controller.AppearanceLightNavigationBars = !dark;
    }
}
