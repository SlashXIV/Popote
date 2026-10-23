using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Popote.Services;

// Une recette au format texte de Popote (import par IA, partage) : lisible, tolérant, sans JSON.
//   # Crêpes de la Chandeleur
//   Portions : 4
//   Préparation : 10 min
//   Cuisson : 20 min
//   Tags : Dessert, Rapide
//   ## Ingrédients
//   - 250 g | Farine | Épicerie
//   ## Étapes
//   1. Verser la farine dans un saladier…
//   ## Notes
//   Moins de sucre la prochaine fois.
// Pur (sans MAUI ni base) : testé dans Popote.Tests. Write puis Parse rend la même recette.
public static class RecipeTextFormat
{
    // Valeur par défaut de Recipe.Servings, quand le texte n'en donne pas.
    public const int DefaultServings = 2;

    private static readonly CultureInfo Fr = CultureInfo.GetCultureInfo("fr-FR");

    private enum Section { Header, Ingredients, Steps, Notes }

    public static TextImport Parse(string? text)
    {
        var warnings = new List<ImportWarning>();
        var title = string.Empty;
        int? servings = null, prep = null, cook = null;
        var tags = new List<string>();
        var ingredients = new List<TextIngredient>();
        var steps = new List<string>();
        var notes = new List<string>();
        var section = Section.Header;

        var lines = (text ?? string.Empty).Replace("\r", string.Empty).Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            var number = i + 1;
            var line = lines[i].Trim();
            if (line.StartsWith("```")) continue; // bloc de code autour de la réponse d'une IA
            if (line.Length == 0)
            {
                if (section == Section.Notes && notes.Count > 0) notes.Add(string.Empty);
                continue;
            }
            if (TrySection(line, out var next)) { section = next; continue; }
            if (line.StartsWith("##")) // titre inconnu (« ## Astuces ») : rangé dans les notes
            {
                section = Section.Notes;
                notes.Add(line.TrimStart('#').Trim());
                continue;
            }
            if (line.StartsWith('#'))
            {
                title = line.TrimStart('#').Trim();
                section = Section.Header;
                continue;
            }

            switch (section)
            {
                case Section.Header:
                    ReadField(line, number, ref servings, ref prep, ref cook, tags, warnings);
                    break;
                case Section.Ingredients:
                    var ingredient = ReadIngredient(line, number, warnings);
                    if (ingredient is not null) ingredients.Add(ingredient);
                    break;
                case Section.Steps:
                    steps.Add(StripBullet(line));
                    break;
                case Section.Notes:
                    notes.Add(line);
                    break;
            }
        }

        if (title.Length == 0) warnings.Add(new(ImportWarningKind.MissingTitle, 0, string.Empty));
        if (servings is null) warnings.Add(new(ImportWarningKind.MissingServings, 0, string.Empty));
        if (ingredients.Count == 0) warnings.Add(new(ImportWarningKind.NoIngredients, 0, string.Empty));
        var stepList = StepParser.Parse(string.Join("\n", steps));
        if (stepList.Count == 0) warnings.Add(new(ImportWarningKind.NoSteps, 0, string.Empty));
        var notesText = string.Join("\n", notes).Trim();

        var recipe = new TextRecipe(title, servings ?? DefaultServings, prep, cook, tags, ingredients,
            stepList, notesText.Length == 0 ? null : notesText);
        return new TextImport(recipe, warnings);
    }

    // Plusieurs recettes à la suite (carnet de notes) : chacune commence à une ligne « # Titre ».
    // Le texte avant le premier titre (introduction d'une IA) est ignoré. Les numéros de ligne
    // des avertissements sont ceux du texte entier.
    public static List<TextImport> ParseMany(string? text)
    {
        var lines = (text ?? string.Empty).Replace("\r", string.Empty).Split('\n');
        var starts = Enumerable.Range(0, lines.Length).Where(i => IsTitle(lines[i].Trim())).ToList();
        var imports = new List<TextImport>();
        for (var k = 0; k < starts.Count; k++)
        {
            var end = k + 1 < starts.Count ? starts[k + 1] : lines.Length;
            var (recipe, warnings) = Parse(string.Join("\n", lines[starts[k]..end]));
            imports.Add(new(recipe, warnings.Select(w => w.Line == 0 ? w : w with { Line = w.Line + starts[k] }).ToList()));
        }
        return imports;
    }

    private static bool IsTitle(string line) => line.StartsWith('#') && !line.StartsWith("##");

    public static string Write(TextRecipe recipe)
    {
        var sb = new StringBuilder();
        sb.Append("# ").AppendLine(recipe.Title);
        sb.Append("Portions : ").AppendLine(recipe.Servings.ToString(Fr));
        if (recipe.PrepMinutes is { } prep) sb.Append("Préparation : ").Append(prep).AppendLine(" min");
        if (recipe.CookMinutes is { } cook) sb.Append("Cuisson : ").Append(cook).AppendLine(" min");
        if (recipe.Tags.Count > 0) sb.Append("Tags : ").AppendLine(string.Join(", ", recipe.Tags));

        sb.AppendLine().AppendLine("## Ingrédients");
        foreach (var i in recipe.Ingredients)
        {
            var amount = i.Quantity > 0 ? i.Quantity.ToString("0.##", Fr) : string.Empty;
            if (!string.IsNullOrWhiteSpace(i.Unit)) amount = (amount + " " + i.Unit).Trim();
            sb.Append("- ").Append(amount.Length > 0 ? amount + " | " : "| ")
              .Append(i.Name).Append(" | ").AppendLine(i.Aisle);
        }

        sb.AppendLine().AppendLine("## Étapes");
        for (var s = 0; s < recipe.Steps.Count; s++)
            sb.Append(s + 1).Append(". ").AppendLine(recipe.Steps[s]);

        if (!string.IsNullOrWhiteSpace(recipe.Notes))
            sb.AppendLine().AppendLine("## Notes").AppendLine(recipe.Notes);
        return sb.ToString().TrimEnd() + "\n";
    }

    // « ## Ingrédients », « Étapes : », « ## Préparation » (en titre, pas « Préparation : 10 min »).
    private static bool TrySection(string line, out Section section)
    {
        section = Section.Header;
        var isHeading = line.StartsWith("##") || line.EndsWith(':');
        if (!isHeading) return false;
        switch (Key(line.TrimStart('#').TrimEnd(':')))
        {
            case "ingredients": section = Section.Ingredients; return true;
            case "etapes" or "preparation" or "instructions": section = Section.Steps; return true;
            case "notes" or "note": section = Section.Notes; return true;
            default: return false;
        }
    }

    private static void ReadField(string line, int number, ref int? servings, ref int? prep, ref int? cook,
        List<string> tags, List<ImportWarning> warnings)
    {
        var colon = line.IndexOf(':');
        if (colon <= 0) return; // phrase d'introduction d'une IA : ignorée
        var value = line[(colon + 1)..].Trim();
        switch (Key(line[..colon]))
        {
            case "portions" or "portion" or "personnes" or "parts":
                var m = Regex.Match(value, @"\d+");
                if (m.Success && int.Parse(m.Value, CultureInfo.InvariantCulture) > 0)
                    servings = int.Parse(m.Value, CultureInfo.InvariantCulture);
                else warnings.Add(new(ImportWarningKind.BadServings, number, value));
                break;
            case "preparation" or "prepa" or "tempsdepreparation":
                prep = ReadMinutes(value, number, warnings);
                break;
            case "cuisson" or "tempsdecuisson":
                cook = ReadMinutes(value, number, warnings);
                break;
            case "tags" or "tag" or "etiquettes":
                foreach (var t in value.Split(',', ';').Select(t => t.Trim()).Where(t => t.Length > 0))
                    if (!tags.Contains(t, StringComparer.OrdinalIgnoreCase)) tags.Add(t);
                break;
        }
    }

    // « 10 min », « 1 h 30 », « 1h30 », « 2 heures », « 45 » (minutes).
    private static int? ReadMinutes(string value, int number, List<ImportWarning> warnings)
    {
        var m = Regex.Match(value, @"^(?:(\d+)\s*h(?:eures?)?)?\s*(?:(\d+)\s*(?:min(?:utes?)?\.?)?)?$", RegexOptions.IgnoreCase);
        if (!m.Success || (!m.Groups[1].Success && !m.Groups[2].Success))
        {
            warnings.Add(new(ImportWarningKind.BadDuration, number, value));
            return null;
        }
        var hours = m.Groups[1].Success ? int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture) : 0;
        var minutes = m.Groups[2].Success ? int.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture) : 0;
        var total = hours * 60 + minutes;
        return total > 0 ? total : null;
    }

    // « - 250 g | Farine | Épicerie » ; sans « | », toute la ligne est le nom.
    private static TextIngredient? ReadIngredient(string line, int number, List<ImportWarning> warnings)
    {
        var parts = StripBullet(line).Split('|').Select(p => p.Trim()).ToArray();
        var (amount, name) = parts.Length >= 2 ? (parts[0], parts[1]) : (string.Empty, parts[0]);
        if (name.Length == 0) return null;

        var (quantity, unit) = ReadAmount(amount, number, warnings);
        var aisle = parts.Length >= 3 && parts[2].Length > 0 ? ReadAisle(parts[2], number, warnings) : RecipeVocabulary.DefaultAisle;
        return new TextIngredient(name, quantity, unit, aisle);
    }

    private static readonly Dictionary<char, double> Fractions = new()
    {
        ['½'] = 0.5, ['¼'] = 0.25, ['¾'] = 0.75, ['⅓'] = 1.0 / 3, ['⅔'] = 2.0 / 3, ['⅛'] = 0.125,
    };

    // « 250 g », « 1,5 kg », « ½ », « 1 ½ c. à soupe », « 3/4 », « pincée » (sans nombre).
    private static (double Quantity, string? Unit) ReadAmount(string amount, int number, List<ImportWarning> warnings)
    {
        var end = 0;
        while (end < amount.Length && (char.IsDigit(amount[end]) || ",./ ".Contains(amount[end]) || Fractions.ContainsKey(amount[end])))
            end++;
        var numberText = amount[..end].Trim();
        var unitText = amount[end..].Trim();

        double quantity = 0;
        if (numberText.Length > 0 && !TryReadNumber(numberText, out quantity))
        {
            warnings.Add(new(ImportWarningKind.BadQuantity, number, amount));
            quantity = 0;
        }
        return (quantity, ReadUnit(unitText, number, warnings));
    }

    private static bool TryReadNumber(string text, out double value)
    {
        value = 0;
        foreach (var f in Fractions.Keys) text = text.Replace(f.ToString(), " " + f); // « 1½ » → « 1 ½ »
        foreach (var token in text.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (token.Length == 1 && Fractions.TryGetValue(token[0], out var f)) { value += f; continue; }
            var slash = token.Split('/');
            if (slash.Length == 2 && double.TryParse(slash[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var a)
                && double.TryParse(slash[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var b) && b != 0)
            { value += a / b; continue; }
            if (double.TryParse(token.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
            { value += d; continue; }
            return false;
        }
        return value > 0;
    }

    // Unités : forme canonique de RecipeVocabulary.Units, synonymes et pluriels admis.
    private static readonly Dictionary<string, string> UnitKeys = BuildUnitKeys();

    private static Dictionary<string, string> BuildUnitKeys()
    {
        var keys = new Dictionary<string, string>();
        foreach (var u in RecipeVocabulary.Units) { keys[Key(u)] = u; keys[Key(u) + "s"] = u; }
        void Add(string unit, params string[] synonyms) { foreach (var s in synonyms) keys[Key(s)] = unit; }
        Add("g", "gr", "gramme", "grammes");
        Add("kg", "kilo", "kilos", "kilogramme", "kilogrammes");
        Add("ml", "millilitre", "millilitres");
        Add("cl", "centilitre", "centilitres");
        Add("L", "litre", "litres");
        Add("c. à soupe", "cs", "càs", "cas", "cuillère à soupe", "cuillères à soupe", "c. a soupe", "tbsp");
        Add("c. à café", "cc", "càc", "cac", "cuillère à café", "cuillères à café", "c. a cafe", "tsp");
        Add("pièce", "pc", "pcs", "unité", "unités");
        return keys;
    }

    private static string? ReadUnit(string unit, int number, List<ImportWarning> warnings)
    {
        if (unit.Length == 0) return null;
        if (UnitKeys.TryGetValue(Key(unit), out var known)) return known;
        warnings.Add(new(ImportWarningKind.UnknownUnit, number, unit)); // gardée telle quelle
        return unit;
    }

    // Rayons : forme canonique de RecipeVocabulary.Aisles, « et » pour « & », quelques synonymes.
    private static readonly Dictionary<string, string> AisleKeys = BuildAisleKeys();

    private static Dictionary<string, string> BuildAisleKeys()
    {
        var keys = RecipeVocabulary.Aisles.ToDictionary(Key, a => a);
        void Add(string aisle, params string[] synonyms) { foreach (var s in synonyms) keys[Key(s)] = aisle; }
        Add("Fruits & légumes", "fruits", "légumes", "primeur");
        Add("Crèmerie", "produits laitiers", "frais", "produits frais");
        Add("Viande & poisson", "viande", "poisson", "boucherie", "poissonnerie");
        return keys;
    }

    private static string ReadAisle(string aisle, int number, List<ImportWarning> warnings)
    {
        if (AisleKeys.TryGetValue(Key(aisle), out var known)) return known;
        warnings.Add(new(ImportWarningKind.UnknownAisle, number, aisle));
        return RecipeVocabulary.DefaultAisle;
    }

    // Clé de comparaison : minuscules, sans accents, sans espaces ni points, « et » = « & ».
    private static string Key(string text)
    {
        var plain = new StringBuilder();
        foreach (var c in text.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD))
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) plain.Append(c);
        // Espaces (insécables comprises : \s les couvre) et points ne comptent pas : « c. à soupe » = « càsoupe ».
        return Regex.Replace(Regex.Replace(plain.ToString(), @"\bet\b", "&"), @"[\s.]", string.Empty);
    }

    // Puces et numérotation en tête de ligne : « - », « * », « • », « 1. », « 2) ».
    private static string StripBullet(string line) => Regex.Replace(line, @"^\s*(?:[-*•]|\d+\s*[.)])\s*", string.Empty);
}

public sealed record TextIngredient(string Name, double Quantity, string? Unit, string Aisle);

public sealed record TextRecipe(
    string Title, int Servings, int? PrepMinutes, int? CookMinutes,
    IReadOnlyList<string> Tags, IReadOnlyList<TextIngredient> Ingredients,
    IReadOnlyList<string> Steps, string? Notes);

public sealed record TextImport(TextRecipe Recipe, IReadOnlyList<ImportWarning> Warnings);

// Ce que l'import n'a pas pu reprendre tel quel. Line = 0 pour un manque global.
// Le libellé affiché vient des ressources de l'app (la couche données ne porte pas de texte d'interface).
public sealed record ImportWarning(ImportWarningKind Kind, int Line, string Value);

public enum ImportWarningKind
{
    MissingTitle, MissingServings, BadServings, BadDuration, BadQuantity,
    UnknownUnit, UnknownAisle, NoIngredients, NoSteps,
}
