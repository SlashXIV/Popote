namespace Popote.Motion;

// Courbe de Bézier cubique (0,0)-(x1,y1)-(x2,y2)-(1,1), comme cubic-bezier() en CSS.
// Pure (sans MAUI) : testée dans Popote.Tests. Sert à construire les Easing de MotionTokens.
public sealed class CubicBezier
{
    private readonly double _x1, _y1, _x2, _y2;

    public CubicBezier(double x1, double y1, double x2, double y2)
    {
        _x1 = x1; _y1 = y1; _x2 = x2; _y2 = y2;
    }

    private static double Coord(double t, double p1, double p2)
    {
        var u = 1 - t;
        return 3 * u * u * t * p1 + 3 * u * t * t * p2 + t * t * t;
    }

    private static double Slope(double t, double p1, double p2)
    {
        var u = 1 - t;
        return 3 * u * u * p1 + 6 * u * t * (p2 - p1) + 3 * t * t * (1 - p2);
    }

    // Progression y pour un temps x dans [0, 1] : Newton, puis dichotomie si la pente s'annule.
    public double Evaluate(double x)
    {
        if (x <= 0) return 0;
        if (x >= 1) return 1;

        var t = x;
        for (var i = 0; i < 8; i++)
        {
            var err = Coord(t, _x1, _x2) - x;
            if (Math.Abs(err) < 1e-6) return Coord(t, _y1, _y2);
            var d = Slope(t, _x1, _x2);
            if (Math.Abs(d) < 1e-6) break;
            t -= err / d;
        }

        double lo = 0, hi = 1;
        t = x;
        for (var i = 0; i < 40; i++)
        {
            var c = Coord(t, _x1, _x2);
            if (Math.Abs(c - x) < 1e-6) break;
            if (c < x) lo = t; else hi = t;
            t = (lo + hi) / 2;
        }
        return Coord(t, _y1, _y2);
    }
}
