using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Storage;
using Popote.Resources.Strings;
using Popote.Services;

namespace Popote.ViewModels;

// Réglages : apparence (Système / Clair / Sombre), Ambiance (Expressive / Sobre), vibrations,
// et données (sauvegarde / restauration, déplacées depuis le menu ⋮ de la liste, logique inchangée).
public partial class SettingsViewModel : ObservableObject
{
    private readonly RecipeService _service;
    private readonly SettingsService _settings;

    public SettingsViewModel(RecipeService service, SettingsService settings)
    {
        _service = service;
        _settings = settings;
        themeIndex = (int)settings.Theme;
        ambianceIndex = settings.Sober ? 1 : 0;
        hapticsEnabled = settings.HapticsEnabled;
    }

    // L'ordre suit l'enum ThemeChoice (System, Light, Dark).
    public string[] ThemeOptions { get; } = { AppResources.Settings_ThemeSystem, AppResources.Settings_ThemeLight, AppResources.Settings_ThemeDark };
    public string[] AmbianceOptions { get; } = { AppResources.Settings_AmbianceExpressive, AppResources.Settings_AmbianceSober };

    public string VersionLabel => string.Format(AppResources.Settings_Version, AppInfo.Current.VersionString);

    [ObservableProperty]
    private int themeIndex;

    [ObservableProperty]
    private int ambianceIndex;

    [ObservableProperty]
    private bool hapticsEnabled;

    partial void OnThemeIndexChanged(int value) => _settings.Theme = (ThemeChoice)value;
    partial void OnAmbianceIndexChanged(int value) => _settings.Sober = value == 1;
    partial void OnHapticsEnabledChanged(bool value) => _settings.HapticsEnabled = value;

    // Exporte la base (.db3) et la partage (Drive, Fichiers, mail…).
    [RelayCommand]
    private async Task BackupAsync()
    {
        await _service.CheckpointAsync(); // WAL -> .db3 (sauvegarde en un seul fichier)

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "recettes.db3");
        if (!File.Exists(dbPath))
        {
            await Shell.Current.DisplayAlertAsync(AppResources.Backup_Title, AppResources.Backup_Empty, AppResources.Action_Ok);
            return;
        }

        var backup = Path.Combine(FileSystem.CacheDirectory, $"popote-sauvegarde-{DateTime.Now:yyyyMMdd-HHmm}.db3");
        File.Copy(dbPath, backup, overwrite: true);

        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = AppResources.Backup_ShareTitle,
            File = new ShareFile(backup)
        });
    }

    // Restaure la base depuis un fichier de sauvegarde choisi.
    [RelayCommand]
    private async Task RestoreAsync()
    {
        var confirm = await Shell.Current.DisplayAlertAsync(
            AppResources.Restore_ConfirmTitle, AppResources.Restore_ConfirmMessage,
            AppResources.Restore_ConfirmAccept, AppResources.Action_Cancel);
        if (!confirm) return;

        var file = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = AppResources.Restore_PickerTitle });
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
        await Shell.Current.DisplayAlertAsync(AppResources.Restore_DoneTitle, AppResources.Restore_DoneMessage, AppResources.Action_Ok);
    }
}
