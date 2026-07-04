using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Storage;
using Popote.Models;
using Popote.Services;

namespace Popote.ViewModels;

// ViewModel de la page liste.
// "partial" est obligatoire : CommunityToolkit.Mvvm génère du code
// (les propriétés et les commandes) à la compilation.
public partial class RecipeListViewModel : ObservableObject
{
    private readonly RecipeService _service;

    public RecipeListViewModel(RecipeService service) => _service = service;

    // Collection observable : la UI se met à jour automatiquement quand on ajoute/retire.
    public ObservableCollection<Recipe> Recipes { get; } = new();

    // Tags de filtre (puces à bascule). Filtre en ET : une recette doit porter
    // tous les tags actifs.
    public ObservableCollection<TagToggleViewModel> FilterTags { get; } = new();

    [ObservableProperty]
    private bool hasTags;

    // Tri (l'ordre doit correspondre à l'enum RecipeSort : Favoris, Récent, Titre, Temps).
    public string[] SortOptions { get; } = { "Favoris", "Récent", "Titre", "Temps" };

    [ObservableProperty]
    private int sortIndex;

    partial void OnSortIndexChanged(int value) => _ = LoadAsync();

    // [ObservableProperty] sur le champ "searchText" génère une propriété "SearchText"
    // qui notifie l'UI à chaque changement.
    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private bool isBusy;

    // [RelayCommand] sur "LoadAsync" génère une commande "LoadCommand"
    // (le suffixe "Async" est retiré automatiquement).
    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            // Rafraîchit les tags de filtre en conservant les sélections actives.
            var selected = FilterTags.Where(t => t.IsSelected).Select(t => t.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var allTags = await _service.GetTagsAsync();
            FilterTags.Clear();
            foreach (var name in allTags)
                FilterTags.Add(new TagToggleViewModel(name, selected.Contains(name)));
            HasTags = FilterTags.Count > 0;

            var activeTags = FilterTags.Where(t => t.IsSelected).Select(t => t.Name).ToList();

            Recipes.Clear();
            var list = await _service.GetRecipesAsync(SearchText, activeTags, (RecipeSort)SortIndex);
            foreach (var r in list)
                Recipes.Add(r);
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Active/désactive un tag de filtre puis recharge la liste.
    [RelayCommand]
    private async Task ToggleFilterAsync(TagToggleViewModel? tag)
    {
        if (tag is null) return;
        tag.IsSelected = !tag.IsSelected;
        await LoadAsync();
    }

    // Navigation vers la page d'édition.
    // recipe == null  -> création (bouton « Ajouter ») ; sinon -> édition.
    [RelayCommand]
    private async Task GoToEditAsync(Recipe? recipe)
    {
        var route = recipe is null
            ? "RecipeEditPage"                       // route enregistrée dans AppShell
            : $"RecipeEditPage?id={recipe.Id}";      // on passe l'id en paramètre
        await Shell.Current.GoToAsync(route);
    }

    // Ouvre « Cuisiner avec… » (recherche par ingrédients).
    [RelayCommand]
    private async Task GoToCookWithAsync() => await Shell.Current.GoToAsync("CookWithPage");

    // Tap sur une recette -> page de consultation (détail).
    [RelayCommand]
    private async Task GoToDetailAsync(Recipe? recipe)
    {
        if (recipe is null) return;
        await Shell.Current.GoToAsync($"RecipeDetailPage?id={recipe.Id}");
    }

    // Supprime une recette (balayage) après confirmation, puis recharge la liste.
    [RelayCommand]
    private async Task DeleteRecipeAsync(Recipe? recipe)
    {
        if (recipe is null) return;

        var confirm = await Shell.Current.DisplayAlertAsync(
            "Supprimer la recette ?",
            $"« {recipe.Title} » sera définitivement supprimée.",
            "Supprimer", "Annuler");
        if (!confirm) return;

        await _service.DeleteRecipeAsync(recipe.Id);
        await LoadAsync();
    }

    // Exporte la base (.db3) et la partage (Drive, Fichiers, mail…).
    [RelayCommand]
    private async Task BackupAsync()
    {
        await _service.CheckpointAsync(); // WAL -> .db3 (sauvegarde en un seul fichier)

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "recettes.db3");
        if (!File.Exists(dbPath))
        {
            await Shell.Current.DisplayAlertAsync("Sauvegarde", "Aucune donnée à sauvegarder.", "OK");
            return;
        }

        var backup = Path.Combine(FileSystem.CacheDirectory, $"popote-sauvegarde-{DateTime.Now:yyyyMMdd-HHmm}.db3");
        File.Copy(dbPath, backup, overwrite: true);

        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = "Sauvegarde Popote",
            File = new ShareFile(backup)
        });
    }

    // Restaure la base depuis un fichier de sauvegarde choisi.
    [RelayCommand]
    private async Task RestoreAsync()
    {
        var confirm = await Shell.Current.DisplayAlertAsync(
            "Restaurer une sauvegarde ?",
            "Tes données actuelles seront remplacées par le fichier choisi.",
            "Restaurer", "Annuler");
        if (!confirm) return;

        var file = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = "Choisis une sauvegarde Popote (.db3)" });
        if (file is null) return;

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "recettes.db3");
        using (var src = await file.OpenReadAsync())
        using (var dst = File.Create(dbPath))
            await src.CopyToAsync(dst);

        // Supprime les journaux WAL périmés de l'ancienne base.
        foreach (var ext in new[] { "-wal", "-shm" })
        {
            var p = dbPath + ext;
            if (File.Exists(p)) File.Delete(p);
        }

        await _service.MigrateAsync(); // aligne le schéma si la sauvegarde est plus ancienne
        await Shell.Current.DisplayAlertAsync("Restauration effectuée",
            "Redémarre l'app pour voir tes données restaurées.", "OK");
    }
}
