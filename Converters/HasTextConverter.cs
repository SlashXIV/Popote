using System.Globalization;

namespace Popote.Converters;

// Renvoie true si la chaîne n'est pas vide — pratique pour n'afficher un élément
// (image, bouton) que si une valeur est renseignée (ex. PhotoPath).
public class HasTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => !string.IsNullOrWhiteSpace(value as string);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
