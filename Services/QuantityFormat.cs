using System.Globalization;

namespace Popote.Services;

// Mise en forme d'une quantité pour l'affichage (« 4 pièces », « 0,5 L ») et pour
// TalkBack (« 200 grammes »). Présentation pure : ne touche ni aux données ni au calcul
// des portions (ServingsScaler). Testé dans Popote.Tests (fichier lié).
public static class QuantityFormat
{
    private static readonly CultureInfo Fr = CultureInfo.GetCultureInfo("fr-FR");

    // Unités qui prennent un « s » au pluriel (les abréviations restent invariables).
    private static readonly HashSet<string> Countable = new(StringComparer.OrdinalIgnoreCase)
        { "pièce", "pincée", "gousse", "tranche", "sachet" };

    // Forme parlée des abréviations (singulier, pluriel).
    private static readonly Dictionary<string, (string One, string Many)> Spoken = new(StringComparer.OrdinalIgnoreCase)
    {
        ["g"] = ("gramme", "grammes"), ["kg"] = ("kilogramme", "kilogrammes"),
        ["ml"] = ("millilitre", "millilitres"), ["cl"] = ("centilitre", "centilitres"), ["L"] = ("litre", "litres"),
        ["c. à soupe"] = ("cuillère à soupe", "cuillères à soupe"), ["c. à café"] = ("cuillère à café", "cuillères à café"),
    };

    public static string Number(double quantity) => quantity.ToString("0.##", Fr);

    // En français, le pluriel commence à 2 (« 1,5 pièce »).
    private static bool IsPlural(double quantity) => quantity >= 2;

    public static string Unit(double quantity, string? unit)
    {
        if (string.IsNullOrWhiteSpace(unit)) return string.Empty;
        return IsPlural(quantity) && Countable.Contains(unit) ? unit + "s" : unit;
    }

    // « 250 g », « 4 pièces », « 3 » ; vide si aucune quantité.
    // Espace insécable entre nombre et unité : « 250 » et « g » ne sont jamais séparés.
    public static string Label(double quantity, string? unit)
    {
        if (quantity <= 0) return string.Empty;
        var u = Unit(quantity, unit);
        return u.Length == 0 ? Number(quantity) : $"{Number(quantity)} {u}";
    }

    // « 200 grammes », « 4 pièces » : pour une description TalkBack.
    public static string SpokenLabel(double quantity, string? unit)
    {
        if (quantity <= 0) return string.Empty;
        if (unit is not null && Spoken.TryGetValue(unit, out var s))
            return $"{Number(quantity)} {(IsPlural(quantity) ? s.Many : s.One)}";
        var u = Unit(quantity, unit);
        return u.Length == 0 ? Number(quantity) : $"{Number(quantity)} {u}";
    }
}
