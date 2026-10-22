namespace Popote.Motion;

// Haptique centralisée : un vocabulaire court et cohérent, coupé par le réglage « Vibrations ».
public static class Haptics
{
    public static bool Enabled { get; set; } = true;

    // Un choix, une coche.
    public static void Tick() => Perform(HapticFeedbackType.Click);

    // Une action aboutie : recette enregistrée, minuteur terminé, entrée en cuisson.
    public static void Confirm() => Perform(HapticFeedbackType.LongPress);

    private static void Perform(HapticFeedbackType type)
    {
        if (!Enabled) return;
        try { HapticFeedback.Default.Perform(type); }
        catch (FeatureNotSupportedException) { /* appareil sans vibreur */ }
        catch (PermissionException) { /* manifeste sans VIBRATE : l'haptique n'est qu'un bonus */ }
    }
}
