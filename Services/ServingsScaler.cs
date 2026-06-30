namespace Popote.Services;

// Petit utilitaire pur (pas d'état, facile à tester unitairement).
// Recalcule une quantité quand on change le nombre de portions.
// Ex : recette de base 2 portions -> 400 g de pois chiches.
//      Pour 5 portions : 400 * 5 / 2 = 1000 g.
public static class ServingsScaler
{
    public static double Scale(double baseQuantity, int baseServings, int targetServings)
    {
        if (baseServings <= 0) return baseQuantity; // garde-fou : pas de division par zéro
        return baseQuantity * targetServings / baseServings;
    }
}
