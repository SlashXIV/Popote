using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Popote.Services;

namespace Popote.ViewModels;

// Mode cuisson : étapes défilables + minuteur, écran maintenu allumé (géré par la page).
[QueryProperty(nameof(RecipeId), "id")]
public partial class CookingModeViewModel : ObservableObject
{
    private readonly RecipeService _service;

    public CookingModeViewModel(RecipeService service) => _service = service;

    [ObservableProperty]
    private int recipeId;

    [ObservableProperty]
    private string title = string.Empty;

    public ObservableCollection<StepLine> Steps { get; } = new();

    // --- Minuteur ---
    private IDispatcherTimer? _timer;
    private int _remainingSeconds;

    [ObservableProperty]
    private string timerMinutes = string.Empty;

    [ObservableProperty]
    private string timerDisplay = string.Empty;

    [ObservableProperty]
    private bool isTimerRunning;

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
        Steps.Clear();
        var number = 1;
        foreach (var step in StepParser.Parse(r.Instructions))
            Steps.Add(new StepLine(number++, step));
    }

    [RelayCommand]
    private void StartTimer()
    {
        if (!int.TryParse(TimerMinutes?.Trim(), out var minutes) || minutes <= 0)
            return;

        _remainingSeconds = minutes * 60;
        UpdateTimerDisplay();

        _timer ??= Application.Current!.Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick -= OnTick;
        _timer.Tick += OnTick;
        _timer.Start();
        IsTimerRunning = true;
    }

    [RelayCommand]
    private void StopTimer()
    {
        _timer?.Stop();
        IsTimerRunning = false;
        TimerDisplay = string.Empty;
    }

    private void OnTick(object? sender, EventArgs e)
    {
        _remainingSeconds--;
        if (_remainingSeconds <= 0)
        {
            _timer?.Stop();
            IsTimerRunning = false;
            TimerDisplay = "Terminé !";
            _ = Shell.Current.DisplayAlertAsync("Minuteur", "C'est prêt ! ⏱", "OK");
            return;
        }
        UpdateTimerDisplay();
    }

    private void UpdateTimerDisplay()
        => TimerDisplay = TimeSpan.FromSeconds(_remainingSeconds).ToString(@"mm\:ss");
}
