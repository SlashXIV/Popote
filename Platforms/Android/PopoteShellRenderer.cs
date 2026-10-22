using Android.Content.Res;
using Android.Widget;
using Google.Android.Material.BottomNavigation;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using AView = Android.Views.View;
using AViewGroup = Android.Views.ViewGroup;

namespace Popote;

// Barre d'onglets Shell gardée (navigation, retour, TalkBack natifs), rendu maison :
// pas de pilule Material ni d'ondulation, libellés en Inter, couleurs des jetons (Shell.TabBar*).
public class PopoteShellRenderer : ShellRenderer
{
    protected override IShellBottomNavViewAppearanceTracker CreateBottomNavViewAppearanceTracker(ShellItem shellItem)
        => new PopoteBottomNavTracker(this, shellItem);
}

internal sealed class PopoteBottomNavTracker : ShellBottomNavViewAppearanceTracker
{
    public PopoteBottomNavTracker(IShellContext context, ShellItem item) : base(context, item) { }

    public override void SetAppearance(BottomNavigationView bottomView, IShellAppearanceElement appearance)
    {
        base.SetAppearance(bottomView, appearance);
        Restyle(bottomView);
    }

    public override void ResetAppearance(BottomNavigationView bottomView)
    {
        base.ResetAppearance(bottomView);
        Restyle(bottomView);
    }

    private static void Restyle(BottomNavigationView view)
    {
        view.ItemActiveIndicatorEnabled = false;
        view.ItemRippleColor = ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
        view.LabelVisibilityMode = LabelVisibilityMode.LabelVisibilityLabeled;
        view.Elevation = 0;

        var fonts = Application.Current?.Handler?.MauiContext?.Services.GetService<IFontManager>();
        var typeface = fonts?.GetTypeface(Microsoft.Maui.Font.OfSize("InterSemiBold", 12));
        if (typeface is not null) ApplyTypeface(view, typeface);
    }

    private static void ApplyTypeface(AView view, Android.Graphics.Typeface typeface)
    {
        if (view is TextView text) text.Typeface = typeface;
        else if (view is AViewGroup group)
            for (var i = 0; i < group.ChildCount; i++)
                ApplyTypeface(group.GetChildAt(i)!, typeface);
    }
}
