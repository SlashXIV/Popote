using Microsoft.EntityFrameworkCore;
using Popote.Models;

namespace Popote.Data;

// Le DbContext est le point d'entrée d'EF Core : il expose les tables (DbSet)
// et traduit ton LINQ en SQL. La connexion SQLite est configurée dans MauiProgram
// (via AddDbContextFactory), donc ici on reçoit juste les "options".
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Une propriété DbSet = une table.
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<RecipeTag> RecipeTags => Set<RecipeTag>();
    public DbSet<PlannedMeal> PlannedMeals => Set<PlannedMeal>();

    // Configuration fine du schéma (ce que les attributs ne suffisent pas à exprimer).
    protected override void OnModelCreating(ModelBuilder mb)
    {
        // Clé primaire composite pour la jointure recette <-> tag.
        mb.Entity<RecipeTag>().HasKey(rt => new { rt.RecipeId, rt.TagId });

        // Un nom d'ingrédient est unique : évite d'avoir "Tomate" en double.
        mb.Entity<Ingredient>()
          .HasIndex(i => i.Name)
          .IsUnique();
    }
}
