using Popote.Models;
using Popote.Services;
using Xunit;

namespace Popote.Tests;

public class ShoppingListBuilderTests
{
    private static RecipeIngredient Line(string name, string? aisle, double qty, string? unit)
        => new()
        {
            Ingredient = new Ingredient { Name = name, Aisle = aisle },
            Quantity = qty,
            Unit = unit
        };

    [Fact]
    public void Aggregate_MemeIngredientMemeUnite_EstSomme()
    {
        var result = ShoppingListBuilder.Aggregate(new[]
        {
            Line("Farine", "Épicerie", 100, "g"),
            Line("Farine", "Épicerie", 150, "g"),
        });

        var farine = Assert.Single(result);
        Assert.Equal(250, farine.Quantity);
        Assert.Equal("g", farine.Unit);
        Assert.Equal("Épicerie", farine.Aisle);
    }

    [Fact]
    public void Aggregate_UnitesDifferentes_RestentSeparees()
    {
        var result = ShoppingListBuilder.Aggregate(new[]
        {
            Line("Lait", "Crèmerie", 200, "ml"),
            Line("Lait", "Crèmerie", 1, "L"),
        });

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Aggregate_RayonAbsent_TombeDansDivers()
    {
        var result = ShoppingListBuilder.Aggregate(new[] { Line("Sel", null, 5, "g") });
        Assert.Equal("Divers", Assert.Single(result).Aisle);
    }

    [Fact]
    public void Aggregate_TriParNom_DansUnMemeRayon()
    {
        var result = ShoppingListBuilder.Aggregate(new[]
        {
            Line("Farine", "Épicerie", 100, "g"),
            Line("Cacao", "Épicerie", 10, "g"),
        });

        Assert.Equal(new[] { "Cacao", "Farine" }, result.Select(r => r.Name).ToArray());
    }
}
