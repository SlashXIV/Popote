using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RecettesApp.Data;
using RecettesApp.Services;
using RecettesApp.ViewModels;
using RecettesApp.Views;

namespace RecettesApp;

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
        // Édition : transient (une instance neuve à chaque ouverture).
        builder.Services.AddTransient<RecipeEditViewModel>();
        builder.Services.AddTransient<RecipeEditPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        // Crée le fichier SQLite + les tables au tout premier lancement.
        // (Suffisant pour apprendre ; on passera aux migrations plus tard si besoin.)
        using (var scope = app.Services.CreateScope())
        {
            var factory = scope.ServiceProvider
                .GetRequiredService<IDbContextFactory<AppDbContext>>();
            using var db = factory.CreateDbContext();
            db.Database.EnsureCreated();
        }

        return app;
    }
}
