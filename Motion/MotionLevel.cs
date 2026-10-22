namespace Popote.Motion;

// Niveau de mouvement de l'app. Pur (sans MAUI) : testé dans Popote.Tests.
// - Full : tout, y compris le décoratif (Ambiance « Expressive »).
// - Reduced : fondus courts seulement (Ambiance « Sobre »).
// - None : rien ne bouge (réglage Android « Supprimer les animations »), qui l'emporte toujours.
public enum MotionLevel { Full, Reduced, None }

public static class MotionLevels
{
    public static MotionLevel Of(float systemAnimatorScale, bool sober)
        => systemAnimatorScale == 0f ? MotionLevel.None
         : sober ? MotionLevel.Reduced
         : MotionLevel.Full;
}
