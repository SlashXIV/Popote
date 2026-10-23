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

    [RelayCommand]
    private Task GoToImportManyAsync() => Shell.Current.GoToAsync("ImportManyPage");

    // Exporte la base et les photos dans une archive .zip, puis la partage (Drive, Fichiers, mail…).
    [RelayCommand]
    private async Task BackupAsync()
    {
        await _service.CheckpointAsync(); // WAL -> .db3 (la base tient en un seul fichier)

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, BackupArchive.DbName);
        if (!File.Exists(dbPath))
        {
            await Shell.Current.DisplayAlertAsync(AppResources.Backup_Title, AppResources.Backup_Empty, AppResources.Action_Ok);
            return;
        }

        var photos = await _service.GetPhotoPathsAsync();
        foreach (var old in Directory.GetFiles(FileSystem.CacheDirectory, "popote-sauvegarde-*"))
            File.Delete(old); // une archive pèse les photos : on ne garde que la dernière
        var backup = Path.Combine(FileSystem.CacheDirectory, $"popote-sauvegarde-{DateTime.Now:yyyyMMdd-HHmm}.zip");
        await Task.Run(() =>
        {
            using var output = File.Create(backup);
            BackupArchive.Write(output, dbPath, photos);
        });

        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = AppResources.Backup_ShareTitle,
            File = new ShareFile(backup)
        });
    }

    // Restaure une sauvegarde (.zip, ou ancien .db3) : fichier vérifié avant d'écraser,
    // base actuelle remise en place si quoi que ce soit échoue.
    [RelayCommand]
    private async Task RestoreAsync()
    {
        var confirm = await Shell.Current.DisplayAlertAsync(
            AppResources.Restore_ConfirmTitle, AppResources.Restore_ConfirmMessage,
            AppResources.Restore_ConfirmAccept, AppResources.Action_Cancel);
        if (!confirm) return;

        var file = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = AppResources.Restore_PickerTitle });
        if (file is null) return;

        var appDir = FileSystem.AppDataDirectory;
        var picked = Path.Combine(FileSystem.CacheDirectory, "sauvegarde-choisie");
        try
        {
            using (var src = await file.OpenReadAsync())
            using (var dst = File.Create(picked))
                await src.CopyToAsync(dst);

            await _service.CheckpointAsync(); // la copie de secours doit contenir tout le journal
            await Task.Run(() => BackupArchive.Restore(picked, appDir));
        }
        catch (InvalidDataException)
        {
            await Shell.Current.DisplayAlertAsync(AppResources.Restore_FailedTitle, AppResources.Restore_Invalid, AppResources.Action_Ok);
            return;
        }
        catch (Exception) // lecture ou écriture impossible : la base d'avant est déjà remise
        {
            await Shell.Current.DisplayAlertAsync(AppResources.Restore_FailedTitle, AppResources.Restore_Failed, AppResources.Action_Ok);
            return;
        }
        finally
        {
            File.Delete(picked);
        }

        try
        {
            await _service.MigrateAsync(); // aligne le schéma si la sauvegarde est plus ancienne
        }
        catch (Exception)
        {
            BackupArchive.Undo(appDir);
            await Shell.Current.DisplayAlertAsync(AppResources.Restore_FailedTitle, AppResources.Restore_Failed, AppResources.Action_Ok);
            return;
        }
        await Shell.Current.DisplayAlertAsync(AppResources.Restore_DoneTitle, AppResources.Restore_DoneMessage, AppResources.Action_Ok);
    }
}
