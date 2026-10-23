using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Popote.Models;
using Popote.Resources.Strings;
using Popote.Services;

namespace Popote.ViewModels;

// Importer plusieurs recettes d'un coup (docs/import-many.md) : coller la réponse d'une IA →
// recettes reconnues avec leurs points à vérifier → cases à cocher → enregistrer la sélection.
// Un titre déjà présent est signalé et décoché : rien n'est jamais écrasé.
public partial class ImportManyViewModel : ObservableObject
{
    private readonly RecipeService _service;

    public ImportManyViewModel(RecipeService service) => _service = service;

    public ObservableCollection<ImportCandidateViewModel> Candidates { get; } = new();

    // Compte rendu du dernier geste (vide = rien à dire) ; TalkBack le lit aussitôt.
    [ObservableProperty]
    private string report = string.Empty;

    partial void OnReportChanged(string value)
    {
        if (value.Length > 0) SemanticScreenReader.Default.Announce(value);
    }

    [RelayCommand]
    private async Task CopyInstructionsAsync()
    {
        var text = RecipeEditViewModel.AiInstructions(await _service.GetTagsAsync());
        await Clipboard.Default.SetTextAsync(text + "\n" + AppResources.Import_AskMany);
        Motion.Haptics.Tick();
        Report = AppResources.Import_Copied;
    }

    [RelayCommand]
    private async Task PasteAsync()
    {
        var text = await Clipboard.Default.GetTextAsync();
        if (string.IsNullOrWhiteSpace(text))
        {
            Report = AppResources.Import_ClipboardEmpty;
            return;
        }

        var imports = RecipeTextFormat.ParseMany(text);
        if (imports.Count == 0)
        {
            Report = AppResources.Import_NothingFound;
            return;
        }

        var titles = (await _service.GetRecipesAsync()).Select(r => r.Title).ToHashSet(StringComparer.OrdinalIgnoreCase);
        Candidates.Clear();
        foreach (var import in imports)
        {
            // Doublon avec la base ou avec une recette plus haut dans le même collage.
            var duplicate = !titles.Add(import.Recipe.Title);
            Candidates.Add(new ImportCandidateViewModel(import, duplicate));
        }
        Motion.Haptics.Confirm();
        Report = string.Format(AppResources.ImportMany_Found, imports.Count);
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        var selected = Candidates.Where(c => c.IsSelected && c.CanSave).ToList();
        if (selected.Count == 0)
        {
            Report = AppResources.ImportMany_NoneSelected;
            return;
        }

        foreach (var c in selected)
        {
            var r = c.Recipe;
            var recipe = new Recipe
            {
                Title = r.Title,
                Servings = r.Servings,
                PrepMinutes = r.PrepMinutes,
                CookMinutes = r.CookMinutes,
                Instructions = string.Join("\n", r.Steps),
                Notes = r.Notes
            };
            var inputs = r.Ingredients.Select(i => new IngredientInput(i.Name, i.Quantity, i.Unit, i.Aisle)).ToList();
            await _service.SaveRecipeAsync(recipe, inputs, r.Tags);
            Candidates.Remove(c);
        }
        Motion.Haptics.Confirm();
        Report = string.Format(AppResources.ImportMany_Saved, selected.Count);
    }
}

// Une recette reconnue dans le collage, à cocher ou non.
public partial class ImportCandidateViewModel : ObservableObject
{
    public ImportCandidateViewModel(TextImport import, bool isDuplicate)
    {
        Recipe = import.Recipe;
        IsDuplicate = isDuplicate;
        CanSave = Recipe.Title.Length > 0;
        isSelected = CanSave && !isDuplicate;

        var lines = import.Warnings.Select(RecipeEditViewModel.Describe).ToList();
        if (isDuplicate) lines.Insert(0, AppResources.ImportMany_Duplicate);
        Warnings = string.Join("\n", lines.Select(l => "• " + l));
    }

    public TextRecipe Recipe { get; }
    public bool IsDuplicate { get; }
    public bool CanSave { get; }

    public string Title => CanSave ? Recipe.Title : AppResources.ImportMany_Untitled;

    public string Summary => string.Format(Recipe.Servings > 1 ? AppResources.Portions_Many : AppResources.Portions_One, Recipe.Servings)
        + " · " + string.Format(AppResources.ImportMany_Counts, Recipe.Ingredients.Count, Recipe.Steps.Count);

    public string Warnings { get; }

    [ObservableProperty]
    private bool isSelected;
}
