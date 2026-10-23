using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Popote.Resources.Strings;
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

    // Progression en bols : étapes faites = pleins, étape en cours = posé, à venir = vides.
    public ObservableCollection<StepProgress> Progress { get; } = new();

    // Étape affichée (position du carrousel).
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressSpoken), nameof(CanGoPrevious), nameof(CanGoNext))]
    private int position;

    public string ProgressSpoken => string.Format(AppResources.Cook_ProgressSpoken, Position + 1, Steps.Count);
    public string StepOfLabel => string.Format(AppResources.Cook_StepOf, Steps.Count);
    public bool CanGoPrevious => Position > 0;
    public bool CanGoNext => Position < Steps.Count - 1;

    partial void OnPositionChanged(int value) => UpdateProgress();

    // Met à jour l'état des bols en place (pas de recréation de vues à chaque étape : mesuré au carrousel).
    private void UpdateProgress()
    {
        if (Progress.Count != Steps.Count)
        {
            Progress.Clear();
            foreach (var _ in Steps) Progress.Add(new StepProgress());
        }
        for (var i = 0; i < Progress.Count; i++)
            Progress[i].State = i < Position ? Controls.BowlState.Full : i == Position ? Controls.BowlState.Placed : Controls.BowlState.Empty;
    }

    [RelayCommand]
    private void Next()
    {
        if (CanGoNext) Position++;
    }

    [RelayCommand]
    private void Previous()
    {
        if (CanGoPrevious) Position--;
    }

    [RelayCommand]
    private Task ExitAsync() => Shell.Current.GoToAsync("..");

    // --- Minuteur ---
    private IDispatcherTimer? _timer;
    private int _remainingSeconds;

    [ObservableProperty]
    private string timerMinutes = string.Empty;

    [ObservableProperty]
    private string timerDisplay = string.Empty;

    [ObservableProperty]
    private bool isTimerRunning;

    // Bol-minuteur : vide au repos, posé pendant le décompte, plein à la fin.
    [ObservableProperty]
    private Controls.BowlState timerBowl = Controls.BowlState.Empty;

    // Moment « minuteur fini » : la vue fait déborder le bol.
    public event EventHandler? TimerFinished;

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
        OnPropertyChanged(nameof(StepOfLabel));
        Position = 0;
        UpdateProgress();
        OnPropertyChanged(nameof(ProgressSpoken));
        OnPropertyChanged(nameof(CanGoNext));
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
        TimerBowl = Controls.BowlState.Placed;
    }

    [RelayCommand]
    private void StopTimer()
    {
        _timer?.Stop();
        IsTimerRunning = false;
        TimerDisplay = string.Empty;
        TimerBowl = Controls.BowlState.Empty;
    }

    private void OnTick(object? sender, EventArgs e)
    {
        _remainingSeconds--;
        if (_remainingSeconds <= 0)
        {
            _timer?.Stop();
            IsTimerRunning = false;
            TimerDisplay = AppResources.Cook_TimerDone;
            TimerBowl = Controls.BowlState.Full;
            TimerFinished?.Invoke(this, EventArgs.Empty);
            Motion.Haptics.Confirm();
            SemanticScreenReader.Default.Announce(AppResources.Cook_TimerDoneSpoken); // une seule annonce, à la fin
            _ = Shell.Current.DisplayAlertAsync(AppResources.Cook_Timer, AppResources.Cook_TimerAlertMessage, AppResources.Action_Ok);
            return;
        }
        UpdateTimerDisplay();
    }

    private void UpdateTimerDisplay()
        => TimerDisplay = TimeSpan.FromSeconds(_remainingSeconds).ToString(@"mm\:ss");
}

// Bol de progression d'une étape en mode cuisson.
public partial class StepProgress : ObservableObject
{
    [ObservableProperty]
    private Controls.BowlState state;
}
