using System.Globalization;

namespace Popote.Converters;

// true -> false (ex. afficher un état vide quand une liste n'a pas d'éléments).
public class InvertBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is not true;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is not true;
}
