using System.Globalization;
using Popote.Controls;
using Popote.Models;
using Popote.Resources.Strings;

namespace Popote.Converters;

// Ce qu'une carte recette affiche, tiré de l'entité Recipe (ConverterParameter) :
// - « meta »   : « 4 portions · 30 min »
// - « aisles » : rayons distincts de ses ingrédients (mise en place miniature, 4 au plus)
// - « a11y »   : phrase TalkBack complète (« Crêpes, favori, 4 portions, 30 minutes »)
public class RecipeCardConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Recipe r) return null;
        var minutes = (r.PrepMinutes ?? 0) + (r.CookMinutes ?? 0);
        var portions = string.Format(r.Servings > 1 ? AppResources.Portions_Many : AppResources.Portions_One, r.Servings);
        return (parameter as string) switch
        {
            "aisles" => r.Ingredients
                .Select(i => i.Ingredient?.Aisle)
                .GroupBy(AislePalette.RoleFor).Select(g => g.First())
                .Take(4).ToList(),
            "a11y" => string.Join(", ", new[]
            {
                r.Title,
                r.IsFavorite ? AppResources.Recipe_FavoriteSpoken : null,
                portions,
                minutes > 0 ? string.Format(AppResources.Minutes_Spoken, minutes) : null,
            }.Where(s => s is not null)),
            _ => minutes > 0 ? $"{portions} · {string.Format(AppResources.Minutes_Short, minutes)}" : portions,
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
