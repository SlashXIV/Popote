using Popote.Motion;

namespace Popote.Tests;

public class MotionTests
{
    [Theory]
    [InlineData(1f, false, MotionLevel.Full)]
    [InlineData(1f, true, MotionLevel.Reduced)]
    [InlineData(0f, false, MotionLevel.None)]   // « Supprimer les animations » l'emporte toujours
    [InlineData(0f, true, MotionLevel.None)]
    [InlineData(0.5f, false, MotionLevel.Full)]
    public void Niveau_de_mouvement(float scale, bool sober, MotionLevel expected)
        => Assert.Equal(expected, MotionLevels.Of(scale, sober));

    [Fact]
    public void Courbe_bornee_de_0_a_1()
    {
        var enter = new CubicBezier(0.2, 0.7, 0.2, 1);
        Assert.Equal(0, enter.Evaluate(0));
        Assert.Equal(1, enter.Evaluate(1));
        Assert.Equal(0, enter.Evaluate(-0.5));
        Assert.Equal(1, enter.Evaluate(1.5));
    }

    [Fact]
    public void Courbe_Enter_monotone_et_rapide_au_depart()
    {
        var enter = new CubicBezier(0.2, 0.7, 0.2, 1);
        var previous = 0.0;
        for (var x = 0.05; x <= 1; x += 0.05)
        {
            var y = enter.Evaluate(x);
            Assert.True(y >= previous - 1e-9, $"non monotone en {x}");
            previous = y;
        }
        Assert.True(enter.Evaluate(0.25) > 0.5, "l'entrée doit être déjà bien engagée au quart du temps");
    }

    [Fact]
    public void Courbe_Release_depasse_puis_revient()
    {
        var release = new CubicBezier(0.2, 0.9, 0.2, 1.2);
        var max = Enumerable.Range(1, 99).Max(i => release.Evaluate(i / 100.0));
        Assert.True(max > 1, "Release doit dépasser 1 (léger rebond)");
        Assert.Equal(1, release.Evaluate(1));
    }

    [Fact]
    public void Courbe_lineaire_reste_lineaire()
    {
        var linear = new CubicBezier(1 / 3.0, 1 / 3.0, 2 / 3.0, 2 / 3.0);
        Assert.Equal(0.3, linear.Evaluate(0.3), 4);
    }
}
