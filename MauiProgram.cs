using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using Popote.Data;
using Popote.Services;
using Popote.ViewModels;
using Popote.Views;

namespace Popote;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        // L'app est en français : dates (« lun. 19 »), décimales (« 0,5 »).
        var fr = CultureInfo.GetCultureInfo("fr-FR");
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = fr;
        CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = fr;

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                // Polices OFL embarquées (docs/design-system.md, licences dans docs/licences/).
                fonts.AddFont("BricolageGrotesque-Bold.ttf", "Bricolage");
                fonts.AddFont("BricolageGrotesque-ExtraBold.ttf", "BricolageHeavy");
                fonts.AddFont("Inter-Regular.ttf", "Inter");
                fonts.AddFont("Inter-Medium.ttf", "InterMedium");
                fonts.AddFont("Inter-SemiBold.ttf", "InterSemiBold");
            })
            .ConfigureMauiHandlers(handlers =>
            {
#if ANDROID
                handlers.AddHandler(typeof(Shell), typeof(PopoteShellRenderer));
#endif
            })
            .ConfigureLifecycleEvents(events =>
            {
#if ANDROID
                // Retour sur une page racine : l'app passe en arrière-plan au lieu de détruire
                // l'activité. La recréer dans le même processus fait planter MAUI
                // (ObjectDisposedException dans Shell à la réouverture, présent depuis la 1.0).
                events.AddAndroid(android => android.OnBackPressed(activity =>
                {
                    var nav = Shell.Current?.Navigation;
                    if (nav is null || nav.NavigationStack.Count > 1 || nav.ModalStack.Count > 0) return false;
                    activity.MoveTaskToBack(true);
                    return true;
                }));
#endif
            });

#if ANDROID
        PopoteHandlers.Configure();
#endif

        // --- Couche DONNÉES ---
        // FileSystem.AppDataDirectory = dossier privé de l'app (persistant) sur Android.
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "recettes.db3");
        builder.Services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        // --- Services ---
        builder.Services.AddSingleton<RecipeService>();
        builder.Services.AddSingleton<SettingsService>();

        // --- ViewModels + Pages (enregistrés pour l'injection de dépendances) ---
        // Liste : singleton (une seule page d'accueil).
        builder.Services.AddSingleton<RecipeListViewModel>();
        builder.Services.AddSingleton<RecipeListPage>();
        // Liste de courses : onglet unique, singleton.
        builder.Services.AddSingleton<ShoppingListViewModel>();
        builder.Services.AddSingleton<ShoppingListPage>();
        // Planning : onglet unique, singleton.
        builder.Services.AddSingleton<PlanningViewModel>();
        builder.Services.AddSingleton<PlanningPage>();
        // Édition : transient (une instance neuve à chaque ouverture).
        builder.Services.AddTransient<RecipeEditViewModel>();
        builder.Services.AddTransient<RecipeEditPage>();
        // Détail (consultation) : transient.
        builder.Services.AddTransient<RecipeDetailViewModel>();
        builder.Services.AddTransient<RecipeDetailPage>();
        // Cuisiner avec… (recherche par ingrédients) : transient.
        builder.Services.AddTransient<CookWithViewModel>();
        builder.Services.AddTransient<CookWithPage>();
        // Mode cuisson : transient.
        builder.Services.AddTransient<CookingModeViewModel>();
        builder.Services.AddTransient<CookingModePage>();
        // Réglages (thème, Ambiance, vibrations, sauvegarde) : transient.
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<SettingsPage>();

        builder.Services.AddTransient<ImportManyViewModel>();
        builder.Services.AddTransient<ImportManyPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        // Applique les migrations EF Core au démarrage : crée la base au premier
        // lancement et fait évoluer le schéma ensuite, sans perdre les données.
        using (var scope = app.Services.CreateScope())
        {
            var factory = scope.ServiceProvider
                .GetRequiredService<IDbContextFactory<AppDbContext>>();
            using var db = factory.CreateDbContext();
            db.Database.Migrate();
        }

        return app;
    }
}
