using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Popote.Data;
using Popote.Services;
using Popote.ViewModels;
using Popote.Views;

namespace Popote;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                // Police du design system (cf. docs/design-system.md).
                fonts.AddFont("Inter-Regular.ttf", "Inter");
                fonts.AddFont("Inter-Medium.ttf", "InterMedium");
                fonts.AddFont("Inter-SemiBold.ttf", "InterSemiBold");
            });

        // --- Couche DONNÉES ---
        // FileSystem.AppDataDirectory = dossier privé de l'app (persistant) sur Android.
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "recettes.db3");
        builder.Services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        // --- Services ---
        builder.Services.AddSingleton<RecipeService>();

        // --- ViewModels + Pages (enregistrés pour l'injection de dépendances) ---
        // Liste : singleton (une seule page d'accueil).
        builder.Services.AddSingleton<RecipeListViewModel>();
        builder.Services.AddSingleton<RecipeListPage>();
        // Liste de courses : onglet unique, singleton.
        builder.Services.AddSingleton<ShoppingListViewModel>();
        builder.Services.AddSingleton<ShoppingListPage>();
        // Édition : transient (une instance neuve à chaque ouverture).
        builder.Services.AddTransient<RecipeEditViewModel>();
        builder.Services.AddTransient<RecipeEditPage>();
        // Détail (consultation) : transient.
        builder.Services.AddTransient<RecipeDetailViewModel>();
        builder.Services.AddTransient<RecipeDetailPage>();

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
