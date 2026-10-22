using Popote.Services;

namespace Popote.Tests;

public class QuantityFormatTests
{
    private const char Nbsp = ' ';

    [Theory]
    [InlineData(4, "pièce", "4 pièces")]
    [InlineData(1, "pièce", "1 pièce")]
    [InlineData(1.5, "pièce", "1,5 pièce")]   // en français, le pluriel commence à 2
    [InlineData(2, "gousse", "2 gousses")]
    [InlineData(250, "g", "250 g")]           // abréviation invariable
    [InlineData(2, "c. à soupe", "2 c. à soupe")]
    [InlineData(0.5, "L", "0,5 L")]           // virgule décimale
    [InlineData(3, null, "3")]
    public void Libelle_affiche_accorde_avec_espace_insecable(double qty, string? unit, string expected)
    {
        var i = expected.IndexOf(' '); // seule l'espace entre nombre et unité est insécable
        Assert.Equal(i < 0 ? expected : expected[..i] + Nbsp + expected[(i + 1)..], QuantityFormat.Label(qty, unit));
    }

    [Fact]
    public void Quantite_nulle_n_affiche_rien() => Assert.Equal(string.Empty, QuantityFormat.Label(0, "g"));

    [Theory]
    [InlineData(200, "g", "200 grammes")]
    [InlineData(1, "g", "1 gramme")]
    [InlineData(50, "cl", "50 centilitres")]
    [InlineData(2, "c. à soupe", "2 cuillères à soupe")]
    [InlineData(4, "pièce", "4 pièces")]
    public void Lecture_talkback_sans_abreviation(double qty, string unit, string expected)
        => Assert.Equal(expected, QuantityFormat.SpokenLabel(qty, unit));
}
