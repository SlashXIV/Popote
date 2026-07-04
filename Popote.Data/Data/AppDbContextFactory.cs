using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Popote.Data;

// Fabrique utilisée UNIQUEMENT au design-time (dotnet ef migrations …).
// L'app, elle, configure le DbContext via l'injection de dépendances (MauiProgram).
// Ici on donne juste une connexion SQLite locale pour que l'outil puisse
// instancier le contexte et générer/évaluer les migrations.
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=popote_design.db3")
            .Options;
        return new AppDbContext(options);
    }
}
