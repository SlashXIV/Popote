namespace Popote.Controls;

public enum A11yRole { None, Button, CheckBox, Switch, Radio }

// Rôle TalkBack d'un contrôle maison (un ContentView n'en a pas) : « bouton », « case à
// cocher » avec son état, etc. Appliqué côté Android par Platforms/Android/Handlers.cs.
public static class A11y
{
    public static readonly BindableProperty RoleProperty = BindableProperty.CreateAttached(
        "Role", typeof(A11yRole), typeof(A11y), A11yRole.None);

    public static readonly BindableProperty IsCheckedProperty = BindableProperty.CreateAttached(
        "IsChecked", typeof(bool), typeof(A11y), false, propertyChanged: OnCheckedChanged);

    public static A11yRole GetRole(BindableObject view) => (A11yRole)view.GetValue(RoleProperty);
    public static void SetRole(BindableObject view, A11yRole value) => view.SetValue(RoleProperty, value);
    public static bool GetIsChecked(BindableObject view) => (bool)view.GetValue(IsCheckedProperty);
    public static void SetIsChecked(BindableObject view, bool value) => view.SetValue(IsCheckedProperty, value);

    // Prévient TalkBack que l'état a changé (sans répéter toute la description).
    private static void OnCheckedChanged(BindableObject view, object oldValue, object newValue)
    {
#if ANDROID
        if (view is VisualElement { Handler.PlatformView: Android.Views.View v })
            v.SendAccessibilityEvent(Android.Views.Accessibility.EventTypes.ViewSelected);
#endif
    }
}
