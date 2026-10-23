using Popote.Services;
using Xunit;

namespace Popote.Tests;

public class RecipeTextFormatTests
{
    private const string Crepes = """
        # Crêpes de la Chandeleur
        Portions : 4
        Préparation : 10 min
        Cuisson : 20 min
        Tags : Dessert, Rapide
        ## Ingrédients
        - 250 g | Farine | Épicerie
        - 4 pièce | Œufs | Crèmerie
        ## Étapes
        1. Verser la farine dans un saladier.
        2. Ajouter les œufs.
        ## Notes
        Moins de sucre la prochaine fois.
        """;

    [Fact]
    public void Parse_FormatComplet_SansAvertissement()
    {
        var (r, warnings) = RecipeTextFormat.Parse(Crepes);

        Assert.Empty(warnings);
        Assert.Equal("Crêpes de la Chandeleur", r.Title);
        Assert.Equal(4, r.Servings);
        Assert.Equal(10, r.PrepMinutes);
        Assert.Equal(20, r.CookMinutes);
        Assert.Equal(new[] { "Dessert", "Rapide" }, r.Tags);
        Assert.Equal(new TextIngredient("Farine", 250, "g", "Épicerie"), r.Ingredients[0]);
        Assert.Equal(new TextIngredient("Œufs", 4, "pièce", "Crèmerie"), r.Ingredients[1]);
        Assert.Equal(new[] { "Verser la farine dans un saladier.", "Ajouter les œufs." }, r.Steps);
        Assert.Equal("Moins de sucre la prochaine fois.", r.Notes);
    }

    [Fact]
    public void Parse_ChampsManquants_ValeursParDefautEtAvertissements()
    {
        var (r, warnings) = RecipeTextFormat.Parse("Voici ta recette :\n## Ingrédients\n- Sel");

        Assert.Equal(string.Empty, r.Title);
        Assert.Equal(RecipeTextFormat.DefaultServings, r.Servings);
        Assert.Null(r.PrepMinutes);
        Assert.Null(r.Notes);
        Assert.Equal(new TextIngredient("Sel", 0, null, "Divers"), Assert.Single(r.Ingredients));
        Assert.Equal(
            new[] { ImportWarningKind.MissingTitle, ImportWarningKind.MissingServings, ImportWarningKind.NoSteps },
            warnings.Select(w => w.Kind));
    }

    [Theory]
    [InlineData("250 gr", 250, "g")]
    [InlineData("1 kilo", 1, "kg")]
    [InlineData("2 cs", 2, "c. à soupe")]
    [InlineData("1 càc", 1, "c. à café")]
    [InlineData("3 cuillères à soupe", 3, "c. à soupe")]
    [InlineData("4 pièces", 4, "pièce")]
    [InlineData("2 gousses", 2, "gousse")]
    [InlineData("50 cl", 50, "cl")]
    [InlineData("1 litre", 1, "L")]
    [InlineData("1,5 kg", 1.5, "kg")]
    [InlineData("1.5 kg", 1.5, "kg")]
    [InlineData("½ L", 0.5, "L")]
    [InlineData("1 ½ c. à soupe", 1.5, "c. à soupe")]
    [InlineData("1½", 1.5, null)]
    [InlineData("3/4 L", 0.75, "L")]
    [InlineData("pincée", 0, "pincée")]
    [InlineData("", 0, null)]
    public void Parse_Quantites_EtUnitesSynonymes(string amount, double quantity, string? unit)
    {
        var (r, warnings) = RecipeTextFormat.Parse($"# T\nPortions : 2\n## Ingrédients\n- {amount} | Farine | Épicerie\n## Étapes\nCuire");

        var i = Assert.Single(r.Ingredients);
        Assert.Equal(quantity, i.Quantity, 3);
        Assert.Equal(unit, i.Unit);
        Assert.Empty(warnings);
    }

    [Fact]
    public void Parse_UniteInconnue_GardeeAvecAvertissement()
    {
        var (r, warnings) = RecipeTextFormat.Parse("# T\nPortions : 2\n## Ingrédients\n- 2 boîtes | Tomates | Épicerie\n## Étapes\nCuire");

        Assert.Equal("boîtes", Assert.Single(r.Ingredients).Unit);
        var w = Assert.Single(warnings);
        Assert.Equal((ImportWarningKind.UnknownUnit, 4, "boîtes"), (w.Kind, w.Line, w.Value));
    }

    [Theory]
    [InlineData("Fruits et légumes", "Fruits & légumes")]
    [InlineData("crèmerie", "Crèmerie")]
    [InlineData("Epicerie", "Épicerie")]
    [InlineData("Boucherie", "Viande & poisson")]
    public void Parse_Rayons_Synonymes(string aisle, string expected)
    {
        var (r, warnings) = RecipeTextFormat.Parse($"# T\nPortions : 2\n## Ingrédients\n- 1 | Truc | {aisle}\n## Étapes\nCuire");

        Assert.Equal(expected, Assert.Single(r.Ingredients).Aisle);
        Assert.Empty(warnings);
    }

    [Fact]
    public void Parse_RayonInconnu_DiversAvecAvertissement()
    {
        var (r, warnings) = RecipeTextFormat.Parse("# T\nPortions : 2\n## Ingrédients\n- 1 | Truc | Rayon bio\n## Étapes\nCuire");

        Assert.Equal("Divers", Assert.Single(r.Ingredients).Aisle);
        Assert.Equal(ImportWarningKind.UnknownAisle, Assert.Single(warnings).Kind);
    }

    [Theory]
    [InlineData("10 min", 10)]
    [InlineData("1 h 30", 90)]
    [InlineData("1h30", 90)]
    [InlineData("2 heures", 120)]
    [InlineData("45", 45)]
    [InlineData("1 h 15 min", 75)]
    public void Parse_Durees(string value, int minutes)
    {
        var (r, _) = RecipeTextFormat.Parse($"# T\nPréparation : {value}");
        Assert.Equal(minutes, r.PrepMinutes);
    }

    [Fact]
    public void Parse_DureeIllisible_Avertissement()
    {
        var (r, warnings) = RecipeTextFormat.Parse("# T\nPortions : 2\nCuisson : longtemps\n## Ingrédients\n- Sel\n## Étapes\nCuire");

        Assert.Null(r.CookMinutes);
        Assert.Equal((ImportWarningKind.BadDuration, 3), (Assert.Single(warnings).Kind, warnings[0].Line));
    }

    [Fact]
    public void Parse_ReponseDIa_BlocDeCodeEtTitresTolerés()
    {
        var text = "Voici la recette :\n```\n# Soupe\nPersonnes : 6\nIngrédients :\n* 1 | Poireau | Légumes\nPréparation :\n- Couper.\n- Cuire.\n## Astuces\nServir chaud.\n```\nBon appétit !";

        var (r, _) = RecipeTextFormat.Parse(text);

        Assert.Equal("Soupe", r.Title);
        Assert.Equal(6, r.Servings);
        Assert.Equal("Fruits & légumes", Assert.Single(r.Ingredients).Aisle);
        Assert.Equal(new[] { "Couper.", "Cuire." }, r.Steps);
        Assert.Equal("Astuces\nServir chaud.\nBon appétit !", r.Notes); // la clôture ``` n’est pas une ligne vide
    }

    [Fact]
    public void Write_PuisParse_RendLaMemeRecette()
    {
        var recipe = new TextRecipe("Tarte", 6, 20, 45, new[] { "Dessert" },
            new[]
            {
                new TextIngredient("Pommes", 1.5, "kg", "Fruits & légumes"),
                new TextIngredient("Sel", 0, "pincée", "Épicerie"),
                new TextIngredient("Beurre", 0, null, "Crèmerie"),
                new TextIngredient("Cannelle", 2, "c. à café", "Épicerie"),
            },
            new[] { "Éplucher.", "Cuire 45 min." }, "Ligne 1\nLigne 2");

        var (back, warnings) = RecipeTextFormat.Parse(RecipeTextFormat.Write(recipe));

        Assert.Empty(warnings);
        Assert.Equal(recipe.Title, back.Title);
        Assert.Equal(recipe.Servings, back.Servings);
        Assert.Equal((recipe.PrepMinutes, recipe.CookMinutes), (back.PrepMinutes, back.CookMinutes));
        Assert.Equal(recipe.Tags, back.Tags);
        Assert.Equal(recipe.Ingredients, back.Ingredients);
        Assert.Equal(recipe.Steps, back.Steps);
        Assert.Equal(recipe.Notes, back.Notes);
    }
}
