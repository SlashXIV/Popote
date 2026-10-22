namespace Popote.Controls;

// Accès aux jetons (AppColors.xaml, Tokens.xaml) depuis le code des contrôles maison.
// Une couleur de rôle « X » existe en paire XLight / XDark : Themed() l'applique avec
// AppThemeBinding, donc elle suit le thème à chaud.
internal static class Tokens
{
    private static ResourceDictionary Res => Application.Current!.Resources;

    public static Color Light(string role) => (Color)Res[role + "Light"];
    public static Color Dark(string role) => (Color)Res[role + "Dark"];

    public static void Themed(this BindableObject target, BindableProperty property, string role)
        => target.SetAppThemeColor(property, Light(role), Dark(role));

    public static double Size(string key) => (double)Res[key];

    public static Style Style(string key) => (Style)Res[key];

    // Une forme neuve à chaque appel : une instance de forme ne se partage pas entre deux Border.
    public static IShape Rounded(string radiusKey) => new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = Size(radiusKey) };
}
