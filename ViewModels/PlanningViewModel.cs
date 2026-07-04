using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Popote.Models;
using Popote.Services;

namespace Popote.ViewModels;

// ViewModel de l'onglet « Semaine » : planifie des recettes par jour et génère
// la liste de courses agrégée de la semaine.
public partial class PlanningViewModel : ObservableObject
{
    private readonly RecipeService _service;

    public PlanningViewModel(RecipeService service)
    {
        _service = service;
        WeekStart = StartOfWeek(DateTime.Today);
    }

    [ObservableProperty]
    private DateTime weekStart;             // lundi de la semaine affichée

    [ObservableProperty]
    private string weekLabel = string.Empty;

    [ObservableProperty]
    private bool hasShoppingList;

    // 7 jours de la semaine.
    public ObservableCollection<PlanningDayViewModel> Days { get; } = new();

    // Liste de courses de la semaine (groupée par rayon), générée à la demande.
    public ObservableCollection<ShoppingAisle> ShoppingList { get; } = new();

    private static DateTime StartOfWeek(DateTime d)
    {
        int diff = ((int)d.DayOfWeek + 6) % 7; // lundi = 0
        return d.Date.AddDays(-diff);
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        var start = WeekStart;
        var end = start.AddDays(7);
        WeekLabel = $"Semaine du {start:dd MMM} au {start.AddDays(6):dd MMM}";

        var meals = await _service.GetPlannedMealsAsync(start, end);

        Days.Clear();
        for (var i = 0; i < 7; i++)
        {
            var date = start.AddDays(i);
            var label = Capitalize(date.ToString("dddd d", CultureInfo.CurrentCulture));
            var day = new PlanningDayViewModel(date, label);
            foreach (var m in meals.Where(m => m.Date.Date == date))
                day.Meals.Add(m);
            Days.Add(day);
        }

        // La liste de courses affichée devient obsolète quand on change de semaine.
        ShoppingList.Clear();
        HasShoppingList = false;
    }

    [RelayCommand]
    private async Task PreviousWeekAsync()
    {
        WeekStart = WeekStart.AddDays(-7);
        await LoadAsync();
    }

    [RelayCommand]
    private async Task NextWeekAsync()
    {
        WeekStart = WeekStart.AddDays(7);
        await LoadAsync();
    }

    // Ajoute une recette au jour choisi (sélection via feuille d'actions).
    [RelayCommand]
    private async Task AddMealAsync(PlanningDayViewModel? day)
    {
        if (day is null) return;

        var recipes = await _service.GetRecipesAsync();
        if (recipes.Count == 0)
        {
            await Shell.Current.DisplayAlertAsync("Aucune recette", "Crée d'abord une recette.", "OK");
            return;
        }

        var titles = recipes.Select(r => r.Title).ToArray();
        var choice = await Shell.Current.DisplayActionSheetAsync($"Ajouter — {day.Label}", "Annuler", null, titles);
        if (string.IsNullOrEmpty(choice) || choice == "Annuler") return;

        var recipe = recipes.FirstOrDefault(r => r.Title == choice);
        if (recipe is null) return;

        await _service.AddPlannedMealAsync(day.Date, recipe.Id);
        await LoadAsync();
    }

    [RelayCommand]
    private async Task RemoveMealAsync(PlannedMeal? meal)
    {
        if (meal is null) return;
        await _service.RemovePlannedMealAsync(meal.Id);
        await LoadAsync();
    }

    // Agrège toutes les recettes planifiées de la semaine en liste de courses.
    [RelayCommand]
    private async Task GenerateShoppingListAsync()
    {
        var ids = Days.SelectMany(d => d.Meals).Select(m => m.RecipeId).Distinct().ToList();

        ShoppingList.Clear();
        if (ids.Count == 0)
        {
            HasShoppingList = false;
            return;
        }

        var items = await _service.BuildShoppingListAsync(ids);
        foreach (var group in items.GroupBy(i => i.Aisle))
            ShoppingList.Add(new ShoppingAisle(group.Key, group.Select(i => new ShoppingItemViewModel(i))));
        HasShoppingList = true;
    }

    private static string Capitalize(string s) => s.Length == 0 ? s : char.ToUpper(s[0]) + s[1..];
}
