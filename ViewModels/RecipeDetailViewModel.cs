using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Popote.Services;

namespace Popote.ViewModels;

// ViewModel de la page détail (consultation).
// On affiche les ingrédients avec une quantité RECALCULÉE selon les portions
// cibles (ServingsScaler), et la préparation en étapes numérotées.
[QueryProperty(nameof(RecipeId), "id")]
public partial class RecipeDetailViewModel : ObservableObject
{
    private readonly RecipeService _service;

    public RecipeDetailViewModel(RecipeService service) => _service = service;

    // Lignes pour les portions de BASE, conservées pour recalculer à la volée.
    private readonly List<BaseLine> _baseLines = new();
    private int _baseServings = 1;

    [ObservableProperty]
    private int recipeId;

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private string? photoPath;

    // Tags de la recette (affichage en puces, lecture seule).
    public ObservableCollection<string> Tags { get; } = new();

    [ObservableProperty]
    private bool hasTags;

    // Temps (ex. « Prépa 15 min · Cuisson 20 min »), vide si non renseigné.
    [ObservableProperty]
    private string timesLabel = string.Empty;

    [ObservableProperty]
    private bool hasTimes;

    // Rappel des portions de base (libellé d'aide).
    [ObservableProperty]
    private string baseServingsLabel = string.Empty;

    // Portions cibles : changer cette valeur recalcule les quantités.
    [ObservableProperty]
    private int targetServings = 1;

    public ObservableCollection<ScaledIngredient> Ingredients { get; } = new();
    public ObservableCollection<StepLine> Steps { get; } = new();

    partial void OnRecipeIdChanged(int value)
    {
        if (value > 0)
            _ = LoadAsync(value);
    }

    private async Task LoadAsync(int id)
    {
        var r = await _service.GetRecipeAsync(id);
        if (r is null) return;

        Title = r.Title;
        PhotoPath = r.PhotoPath;

        Tags.Clear();
        foreach (var rt in r.RecipeTags)
            Tags.Add(rt.Tag.Name);
        HasTags = Tags.Count > 0;

        var times = new List<string>();
        if (r.PrepMinutes is int p && p > 0) times.Add($"Prépa {p} min");
        if (r.CookMinutes is int c && c > 0) times.Add($"Cuisson {c} min");
        TimesLabel = string.Join("  ·  ", times);
        HasTimes = times.Count > 0;

        _baseServings = r.Servings <= 0 ? 1 : r.Servings;
        BaseServingsLabel = $"Recette de base : {_baseServings} portion" + (_baseServings > 1 ? "s" : "");

        _baseLines.Clear();
        foreach (var ri in r.Ingredients)
            _baseLines.Add(new BaseLine(ri.Ingredient.Name, ri.Ingredient.Aisle, ri.Quantity, ri.Unit));

        // Préparation : une ligne non vide = une étape.
        // On retire une éventuelle numérotation déjà tapée (« 1. », « 2) »…)
        // pour ne pas doublonner avec le badge numéroté.
        Steps.Clear();
        var number = 1;
        foreach (var step in (r.Instructions ?? string.Empty)
                     .Split('\n')
                     .Select(s => StripLeadingNumber(s.Trim()))
                     .Where(s => s.Length > 0))
            Steps.Add(new StepLine(number++, step));

        TargetServings = _baseServings; // déclenche le recalcul
        RebuildIngredients();           // garantit le calcul même si la valeur n'a pas changé
    }

    partial void OnTargetServingsChanged(int value) => RebuildIngredients();

    private void RebuildIngredients()
    {
        Ingredients.Clear();
        var target = TargetServings <= 0 ? 1 : TargetServings;
        foreach (var b in _baseLines)
        {
            var qty = ServingsScaler.Scale(b.Quantity, _baseServings, target);
            var qtyText = qty.ToString("0.##", CultureInfo.InvariantCulture);
            var label = string.IsNullOrWhiteSpace(b.Unit) ? qtyText : $"{qtyText} {b.Unit}";
            Ingredients.Add(new ScaledIngredient(b.Name, b.Aisle, label));
        }
    }

    // Retire une numérotation en tête de ligne : « 1. », « 2) », « 3 - »…
    private static string StripLeadingNumber(string line)
        => Regex.Replace(line, @"^\s*\d+\s*[.)\-–]\s*", "");

    // Multiplie la recette par rapport aux portions de BASE (×½, ×1, ×2, ×3…).
    // Plus intuitif que de saisir un nombre absolu de portions.
    [RelayCommand]
    private void Multiply(string? factor)
    {
        if (double.TryParse(factor, NumberStyles.Any, CultureInfo.InvariantCulture, out var f) && f > 0)
            TargetServings = Math.Max(1, (int)Math.Round(_baseServings * f));
    }

    [RelayCommand]
    private Task GoToEditAsync() => Shell.Current.GoToAsync($"RecipeEditPage?id={RecipeId}");

    // Ligne d'ingrédient aux portions de base (interne).
    private record BaseLine(string Name, string? Aisle, double Quantity, string? Unit);
}

// Étape de préparation numérotée (affichage).
public record StepLine(int Number, string Text);

// Ingrédient avec quantité déjà mise à l'échelle (affichage).
public record ScaledIngredient(string Name, string? Aisle, string QuantityLabel);
