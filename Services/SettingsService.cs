using Popote.Motion;

namespace Popote.Services;

public enum ThemeChoice { System, Light, Dark }

// Réglages d'interface persistés (Preferences) : thème, Ambiance, vibrations.
// Appliqués à chaud : thème de l'app, niveau de mouvement, haptique.
public class SettingsService
{
    private const string ThemeKey = "ui.theme", SoberKey = "ui.sober", HapticsKey = "ui.haptics";

    public ThemeChoice Theme
    {
        get => (ThemeChoice)Preferences.Default.Get(ThemeKey, (int)ThemeChoice.System);
        set { Preferences.Default.Set(ThemeKey, (int)value); ApplyTheme(); }
    }

    public bool Sober
    {
        get => Preferences.Default.Get(SoberKey, false);
        set { Preferences.Default.Set(SoberKey, value); MotionTokens.Sober = value; }
    }

    public bool HapticsEnabled
    {
        get => Preferences.Default.Get(HapticsKey, true);
        set { Preferences.Default.Set(HapticsKey, value); Haptics.Enabled = value; }
    }

    // Au démarrage : pousse les réglages enregistrés vers l'app.
    public void ApplyAll()
    {
        MotionTokens.Sober = Sober;
        Haptics.Enabled = HapticsEnabled;
        MotionTokens.RefreshSystemScale();
        ApplyTheme();
    }

    private void ApplyTheme()
    {
        if (Application.Current is null) return;
        Application.Current.UserAppTheme = Theme switch
        {
            ThemeChoice.Light => AppTheme.Light,
            ThemeChoice.Dark => AppTheme.Dark,
            _ => AppTheme.Unspecified,
        };
    }
}
