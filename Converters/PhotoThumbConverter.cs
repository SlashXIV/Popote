using System.Globalization;
using Popote.Services;

namespace Popote.Converters;

// PhotoPath → miniature si elle existe (liste des recettes), sinon l'original.
public class PhotoThumbConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => PhotoThumbnails.Best(value as string) is { Length: > 0 } path ? ImageSource.FromFile(path) : null;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
