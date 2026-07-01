using System.Globalization;

namespace Popote.Converters;

// Convertit un nom de rayon (Ingredient.Aisle) en couleur de pastille.
// Couleurs alignées sur la palette « earthy naturals » (cf. docs/design-system.md).
// Rayon inconnu ou vide -> gris « Divers ».
public class AisleToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var aisle = (value as string)?.Trim().ToLowerInvariant();
        return aisle switch
        {
            "fruits & légumes" or "fruits et légumes" or "légumes" or "fruits" => Color.FromArgb("#8CB369"),
            "crèmerie" or "cremerie" or "frais" or "produits frais" => Color.FromArgb("#F4E285"),
            "épicerie" or "epicerie" => Color.FromArgb("#F4A259"),
            "viande & poisson" or "viande et poisson" or "viande" or "poisson" or "boucherie" => Color.FromArgb("#BC4B51"),
            "surgelés" or "surgeles" => Color.FromArgb("#5B8E7D"),
            _ => Color.FromArgb("#9AA8A2"),
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
