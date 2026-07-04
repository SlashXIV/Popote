using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Media;
using Microsoft.Maui.Storage;
using Popote.Models;
using Popote.Services;

namespace Popote.ViewModels;

// [QueryProperty] récupère le paramètre "id" passé dans l'URL de navigation
// (ex : "RecipeEditPage?id=3") et l'injecte dans la propriété RecipeId.
[QueryProperty(nameof(RecipeId), "id")]
public partial class RecipeEditViewModel : ObservableObject
{
    private readonly RecipeService _service;
    private readonly Task _catalogsReady;

    public RecipeEditViewModel(RecipeService service)
    {
        _service = service;
        _catalogsReady = LoadCatalogsAsync(); // suggestions d'ingrédients + tags
    }

    // Ingrédients déjà connus (catalogue : nom + rayon), proposés en chips cliquables.
    public ObservableCollection<IngredientCatalogItem> KnownIngredients { get; } = new();

    [ObservableProperty]
    private bool hasKnownIngredients;

    // Tags disponibles en puces à bascule (sélectionné = présent sur la recette).
    public ObservableCollection<TagToggleViewModel> Tags { get; } = new();

    [ObservableProperty]
    private string newTagText = string.Empty;

    private async Task LoadCatalogsAsync()
    {
        var items = await _service.GetIngredientCatalogAsync();
        KnownIngredients.Clear();
        foreach (var item in items)
            KnownIngredients.Add(item);
        HasKnownIngredients = KnownIngredients.Count > 0;

        var tags = await _service.GetTagsAsync();
        Tags.Clear();
        foreach (var name in tags)
            Tags.Add(new TagToggleViewModel(name));
    }

    // Bascule un tag (présent/absent sur la recette).
    [RelayCommand]
    private void ToggleTag(TagToggleViewModel? tag)
    {
        if (tag is not null)
            tag.IsSelected = !tag.IsSelected;
    }

    // Crée un nouveau tag (ou sélectionne l'existant) depuis le champ de saisie.
    [RelayCommand]
    private void AddNewTag()
    {
        var name = NewTagText?.Trim();
        if (string.IsNullOrWhiteSpace(name)) return;

        var existing = Tags.FirstOrDefault(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase));
        if (existing is not null)
            existing.IsSelected = true;
        else
            Tags.Add(new TagToggleViewModel(name, isSelected: true));

        NewTagText = string.Empty;
    }

    // Tap sur une suggestion -> ajoute une ligne pré-remplie (nom + rayon connu).
    [RelayCommand]
    private void AddKnownIngredient(IngredientCatalogItem? item)
    {
        if (item is not null && !string.IsNullOrWhiteSpace(item.Name))
            Ingredients.Add(new IngredientLineViewModel { Name = item.Name, Aisle = item.Aisle });
    }

    [ObservableProperty]
    private int recipeId;            // 0 = création, >0 = édition

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private string? instructions;

    [ObservableProperty]
    private int servings = 2;

    // Chemin local de la photo du plat (copiée dans le dossier privé de l'app).
    [ObservableProperty]
    private string? photoPath;

    // Temps en minutes (saisis en texte ; vide = non renseigné).
    [ObservableProperty]
    private string prepMinutesText = string.Empty;

    [ObservableProperty]
    private string cookMinutesText = string.Empty;

    // Les lignes d'ingrédients éditables (nom + quantité + unité).
    public ObservableCollection<IngredientLineViewModel> Ingredients { get; } = new();

    // --- Photo ---
    [RelayCommand]
    private async Task PickPhotoAsync()
    {
        try
        {
            var photo = await MediaPicker.Default.PickPhotoAsync();
            if (photo is not null)
                PhotoPath = await CopyToAppDataAsync(photo);
        }
        catch (Exception) { /* annulé ou non supporté */ }
    }

    [RelayCommand]
    private async Task TakePhotoAsync()
    {
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported) return;
            var photo = await MediaPicker.Default.CapturePhotoAsync();
            if (photo is not null)
                PhotoPath = await CopyToAppDataAsync(photo);
        }
        catch (Exception) { /* permission refusée ou annulé */ }
    }

    [RelayCommand]
    private void RemovePhoto() => PhotoPath = null;

    // Copie le fichier choisi dans le dossier privé de l'app (persistant).
    private static async Task<string> CopyToAppDataAsync(FileResult photo)
    {
        var ext = Path.GetExtension(photo.FileName);
        if (string.IsNullOrEmpty(ext)) ext = ".jpg";
        var dest = Path.Combine(FileSystem.AppDataDirectory, $"recipe_{Guid.NewGuid():N}{ext}");

        using var src = await photo.OpenReadAsync();
        using var dst = File.Create(dest);
        await src.CopyToAsync(dst);
        return dest;
    }

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
        PhotoPath = r.PhotoPath;
        PrepMinutesText = r.PrepMinutes?.ToString() ?? string.Empty;
        CookMinutesText = r.CookMinutes?.ToString() ?? string.Empty;

        // Coche les tags de la recette (une fois le catalogue de tags chargé).
        await _catalogsReady;
        var recipeTags = r.RecipeTags.Select(rt => rt.Tag.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var toggle in Tags)
            toggle.IsSelected = recipeTags.Contains(toggle.Name);

        Ingredients.Clear();
        foreach (var ri in r.Ingredients)
        {
            Ingredients.Add(new IngredientLineViewModel
            {
                Name = ri.Ingredient.Name,
                QuantityText = FormatQuantity(ri.Quantity),
                Unit = ri.Unit,
                Aisle = ri.Ingredient.Aisle
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
            Servings = Servings,
            PhotoPath = PhotoPath,
            PrepMinutes = ParseMinutes(PrepMinutesText),
            CookMinutes = ParseMinutes(CookMinutesText)
        };

        // On ne garde que les lignes avec un nom ; la quantité est parsée ici.
        var inputs = Ingredients
            .Where(l => !string.IsNullOrWhiteSpace(l.Name))
            .Select(l => new IngredientInput(l.Name, ParseQuantity(l.QuantityText), l.Unit, l.Aisle))
            .ToList();

        var tagNames = Tags.Where(t => t.IsSelected).Select(t => t.Name).ToList();

        await _service.SaveRecipeAsync(recipe, inputs, tagNames);
        await Shell.Current.GoToAsync(".."); // ".." = retour à la page précédente (la liste)
    }

    // Minutes : entier positif, sinon null (non renseigné).
    private static int? ParseMinutes(string? text)
        => int.TryParse(text?.Trim(), out var m) && m > 0 ? m : null;

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
