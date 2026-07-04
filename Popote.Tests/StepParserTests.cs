using Popote.Services;
using Xunit;

namespace Popote.Tests;

public class StepParserTests
{
    [Fact]
    public void Parse_UneLigneParEtape_RetireLaNumerotation()
    {
        var steps = StepParser.Parse("1. Mélanger\n2. Cuire\n3) Servir");
        Assert.Equal(new[] { "Mélanger", "Cuire", "Servir" }, steps);
    }

    [Fact]
    public void Parse_IgnoreLesLignesVides()
    {
        Assert.Equal(new[] { "Étape" }, StepParser.Parse("  \n Étape \n   "));
    }

    [Fact]
    public void Parse_Null_RenvoieListeVide()
    {
        Assert.Empty(StepParser.Parse(null));
    }
}
