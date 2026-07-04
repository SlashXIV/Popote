using Popote.Services;
using Xunit;

namespace Popote.Tests;

public class ServingsScalerTests
{
    [Fact]
    public void Scale_DoubleLesPortions_DoubleLaQuantite()
    {
        // 400 g pour 2 portions -> 4 portions = 800 g
        Assert.Equal(800, ServingsScaler.Scale(400, baseServings: 2, targetServings: 4));
    }

    [Fact]
    public void Scale_MemesPortions_QuantiteInchangee()
    {
        Assert.Equal(150, ServingsScaler.Scale(150, 3, 3));
    }

    [Theory]
    [InlineData(400, 2, 1, 200)]
    [InlineData(100, 4, 2, 50)]
    [InlineData(90, 2, 5, 225)]
    public void Scale_EstProportionnel(double baseQty, int baseServ, int target, double expected)
    {
        Assert.Equal(expected, ServingsScaler.Scale(baseQty, baseServ, target));
    }

    [Fact]
    public void Scale_PortionsDeBaseZero_RenvoieLaQuantiteDeBase()
    {
        // garde-fou anti division par zéro
        Assert.Equal(400, ServingsScaler.Scale(400, 0, 4));
    }
}
