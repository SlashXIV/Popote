using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Popote.Models;
using Popote.Services;

namespace Popote.ViewModels;

// [QueryProperty] récupère le paramètre "id" passé dans l'URL de navigation
// (ex : "RecipeEditPage?id=3") et l'injecte dans la propriété RecipeId.
[QueryProperty(nameof(RecipeId), "id")]
public partial class RecipeEditViewModel : ObservableObject
{
    private readonly RecipeService _service;

    public RecipeEditViewModel(RecipeService service) => _service = service;

    [ObservableProperty]
    private int recipeId;            // 0 = création, >0 = édition

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private string? instructions;

    [ObservableProperty]
    private int servings = 2;

    // Les lignes d'ingrédients éditables (nom + quantité + unité).
    public ObservableCollection<IngredientLineViewModel> Ingredients { get; } = new();

    // Méthode partielle générée : appelée automatiquement quand RecipeId change.
    // Si on édite une recette existante, on charge ses données.
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
        Instructions = r.Instructions;
        Servings = r.Servings;

        Ingredients.Clear();
        foreach (var ri in r.Ingredients)
        {
            Ingredients.Add(new IngredientLineViewModel
            {
                Name = ri.Ingredient.Name,
                QuantityText = FormatQuantity(ri.Quantity),
                Unit = ri.Unit
            });
        }
    }

    // Ajoute une ligne vide à remplir.
    [RelayCommand]
    private void AddIngredient() => Ingredients.Add(new IngredientLineViewModel());

    // Retire la ligne passée en paramètre (bouton ✕ de chaque ligne).
    [RelayCommand]
    private void RemoveIngredient(IngredientLineViewModel? line)
    {
        if (line is not null)
            Ingredients.Remove(line);
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Title))
            return; // garde-fou minimal : pas de recette sans titre

        var recipe = new Recipe
        {
            Id = RecipeId,
            Title = Title.Trim(),
            Instructions = Instructions,
            Servings = Servings
        };

        // On ne garde que les lignes avec un nom ; la quantité est parsée ici.
        var inputs = Ingredients
            .Where(l => !string.IsNullOrWhiteSpace(l.Name))
            .Select(l => new IngredientInput(l.Name, ParseQuantity(l.QuantityText), l.Unit))
            .ToList();

        await _service.SaveRecipeAsync(recipe, inputs);
        await Shell.Current.GoToAsync(".."); // ".." = retour à la page précédente (la liste)
    }

    // Tolère la virgule ou le point comme séparateur décimal ; renvoie 0 si vide/invalide.
    private static double ParseQuantity(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;

        var normalized = text.Trim().Replace(',', '.');
        return double.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out var value)
            ? value
            : 0;
    }

    // Affichage d'une quantité chargée : pas de zéro inutile, point décimal neutre.
    private static string FormatQuantity(double quantity)
        => quantity == 0 ? string.Empty : quantity.ToString(CultureInfo.InvariantCulture);
}
