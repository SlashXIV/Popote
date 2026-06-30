using System.Collections.ObjectModel;
using System.Globalization;
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
        _baseServings = r.Servings <= 0 ? 1 : r.Servings;

        _baseLines.Clear();
        foreach (var ri in r.Ingredients)
            _baseLines.Add(new BaseLine(ri.Ingredient.Name, ri.Ingredient.Aisle, ri.Quantity, ri.Unit));

        // Préparation : une ligne non vide = une étape.
        Steps.Clear();
        var number = 1;
        foreach (var step in (r.Instructions ?? string.Empty)
                     .Split('\n')
                     .Select(s => s.Trim())
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

    [RelayCommand]
    private Task GoToEditAsync() => Shell.Current.GoToAsync($"RecipeEditPage?id={RecipeId}");

    // Ligne d'ingrédient aux portions de base (interne).
    private record BaseLine(string Name, string? Aisle, double Quantity, string? Unit);
}

// Étape de préparation numérotée (affichage).
public record StepLine(int Number, string Text);

// Ingrédient avec quantité déjà mise à l'échelle (affichage).
public record ScaledIngredient(string Name, string? Aisle, string QuantityLabel);
