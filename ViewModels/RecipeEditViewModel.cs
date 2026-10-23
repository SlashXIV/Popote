using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Media;
using Microsoft.Maui.Storage;
using Popote.Models;
using Popote.Resources.Strings;
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
    [NotifyPropertyChangedFor(nameof(PageTitle))]
    private int recipeId;            // 0 = création, >0 = édition

    public string PageTitle => RecipeId > 0 ? AppResources.Edit_TitleEdit : AppResources.Edit_TitleNew;

    [ObservableProperty]
    private string title = string.Empty;

    // Message sous le champ Titre quand on tente d'enregistrer sans titre.
    [ObservableProperty]
    private string? titleError;

    partial void OnTitleChanged(string value)
    {
        if (!string.IsNullOrWhiteSpace(value)) TitleError = null;
    }

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

    [ObservableProperty]
    private bool isFavorite;

    [ObservableProperty]
    private string? notes;

    // Les lignes d'ingrédients éditables (nom + quantité + unité).
    public ObservableCollection<IngredientLineViewModel> Ingredients { get; } = new();

    // --- Photo ---
    [RelayCommand]
    private async Task PickPhotoAsync()
    {
        try
        {
            var photos = await MediaPicker.Default.PickPhotosAsync();
            var photo = photos?.FirstOrDefault();
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
        IsFavorite = r.IsFavorite;
        Notes = r.Notes;

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

    // --- Import par IA (docs/ai-import.md) : le presse-papiers fait le lien, le formulaire sert d'aperçu ---

    // Compte rendu du dernier copier / coller (vide = rien à dire).
    [ObservableProperty]
    private string importReport = string.Empty;

    // Le compte rendu apparaît après un geste : TalkBack le lit aussitôt.
    partial void OnImportReportChanged(string value)
    {
        if (value.Length > 0) SemanticScreenReader.Default.Announce(value);
    }

    [RelayCommand]
    private async Task CopyAiInstructionsAsync()
    {
        await _catalogsReady;
        var text = AiInstructions(Tags.Select(t => t.Name).ToList());
        // Recette déjà commencée : l'IA la remodèle au lieu d'en créer une.
        text += "\n" + (string.IsNullOrWhiteSpace(Title)
            ? AppResources.Import_AskNew
            : string.Format(AppResources.Import_AskRemodel, RecipeTextFormat.Write(ToText())));
        await Clipboard.Default.SetTextAsync(text);
        Motion.Haptics.Tick();
        ImportReport = AppResources.Import_Copied;
    }

    [RelayCommand]
    private async Task PasteAiAnswerAsync()
    {
        var text = await Clipboard.Default.GetTextAsync();
        if (string.IsNullOrWhiteSpace(text))
        {
            ImportReport = AppResources.Import_ClipboardEmpty;
            return;
        }

        var (recipe, warnings) = RecipeTextFormat.Parse(text);
        if (recipe.Title.Length == 0 && recipe.Ingredients.Count == 0 && recipe.Steps.Count == 0)
        {
            ImportReport = AppResources.Import_NothingFound; // le formulaire n'est pas touché
            return;
        }

        await _catalogsReady;
        Apply(recipe);
        Motion.Haptics.Confirm();
        ImportReport = warnings.Count == 0
            ? AppResources.Import_Done
            : AppResources.Import_DoneWithWarnings + "\n" + string.Join("\n", warnings.Select(w => "• " + Describe(w)));
    }

    // Remplace le contenu du formulaire par la recette collée (photo et favori gardés).
    private void Apply(TextRecipe r)
    {
        if (r.Title.Length > 0) Title = r.Title;
        Servings = r.Servings;
        PrepMinutesText = r.PrepMinutes?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        CookMinutesText = r.CookMinutes?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        Instructions = string.Join("\n", r.Steps);
        Notes = r.Notes;

        foreach (var toggle in Tags)
            toggle.IsSelected = r.Tags.Contains(toggle.Name, StringComparer.OrdinalIgnoreCase);
        foreach (var name in r.Tags.Where(n => !Tags.Any(t => string.Equals(t.Name, n, StringComparison.OrdinalIgnoreCase))))
            Tags.Add(new TagToggleViewModel(name, isSelected: true));

        Ingredients.Clear();
        foreach (var i in r.Ingredients)
            Ingredients.Add(new IngredientLineViewModel
            {
                Name = i.Name,
                QuantityText = FormatQuantity(i.Quantity),
                Unit = i.Unit,
                Aisle = i.Aisle
            });
    }

    // Le formulaire tel quel, au format texte (consignes « remodeler »).
    private TextRecipe ToText() => new(
        Title.Trim(), Servings, ParseMinutes(PrepMinutesText), ParseMinutes(CookMinutesText),
        Tags.Where(t => t.IsSelected).Select(t => t.Name).ToList(),
        Ingredients.Where(l => !string.IsNullOrWhiteSpace(l.Name))
            .Select(l => new TextIngredient(l.Name.Trim(), ParseQuantity(l.QuantityText), l.Unit,
                string.IsNullOrWhiteSpace(l.Aisle) ? RecipeVocabulary.DefaultAisle : l.Aisle))
            .ToList(),
        StepParser.Parse(Instructions), string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim());

    // Consignes pour l'IA (format, unités, rayons, tags existants) ; partagées avec l'import multiple.
    internal static string AiInstructions(IReadOnlyList<string> tags) => string.Format(AppResources.Import_Instructions,
        string.Join(", ", RecipeVocabulary.Units), string.Join(", ", RecipeVocabulary.Aisles),
        tags.Count > 0 ? string.Join(", ", tags) : AppResources.Import_NoTags);

    internal static string Describe(ImportWarning w) => string.Format(w.Kind switch
    {
        ImportWarningKind.MissingTitle => AppResources.Import_Warn_MissingTitle,
        ImportWarningKind.MissingServings => AppResources.Import_Warn_MissingServings,
        ImportWarningKind.BadServings => AppResources.Import_Warn_BadServings,
        ImportWarningKind.BadDuration => AppResources.Import_Warn_BadDuration,
        ImportWarningKind.BadQuantity => AppResources.Import_Warn_BadQuantity,
        ImportWarningKind.UnknownUnit => AppResources.Import_Warn_UnknownUnit,
        ImportWarningKind.UnknownAisle => AppResources.Import_Warn_UnknownAisle,
        ImportWarningKind.NoIngredients => AppResources.Import_Warn_NoIngredients,
        _ => AppResources.Import_Warn_NoSteps,
    }, w.Line, w.Kind == ImportWarningKind.MissingServings ? RecipeTextFormat.DefaultServings.ToString(CultureInfo.InvariantCulture) : w.Value);

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

    private const int SavedPauseMs = 400;

    [ObservableProperty]
    private bool isSaved;

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Title))
        {
            TitleError = AppResources.Edit_TitleRequired; // pas de recette sans titre
            return;
        }

        var recipe = new Recipe
        {
            Id = RecipeId,
            Title = Title.Trim(),
            Instructions = Instructions,
            Servings = Servings,
            PhotoPath = PhotoPath,
            PrepMinutes = ParseMinutes(PrepMinutesText),
            CookMinutes = ParseMinutes(CookMinutesText),
            IsFavorite = IsFavorite,
            Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim()
        };

        // On ne garde que les lignes avec un nom ; la quantité est parsée ici.
        var inputs = Ingredients
            .Where(l => !string.IsNullOrWhiteSpace(l.Name))
            .Select(l => new IngredientInput(l.Name, ParseQuantity(l.QuantityText), l.Unit, l.Aisle))
            .ToList();

        var tagNames = Tags.Where(t => t.IsSelected).Select(t => t.Name).ToList();

        await _service.SaveRecipeAsync(recipe, inputs, tagNames);

        // Moment « recette enregistrée » : le bouton devient une coche, le temps de la voir.
        IsSaved = true;
        Motion.Haptics.Confirm();
        if (Motion.MotionTokens.Level != Motion.MotionLevel.None)
            await Task.Delay(SavedPauseMs);
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
