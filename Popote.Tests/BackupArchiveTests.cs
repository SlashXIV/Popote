using System.IO.Compression;
using Microsoft.Data.Sqlite;
using Popote.Services;
using Xunit;

namespace Popote.Tests;

public class BackupArchiveTests : IDisposable
{
    private readonly string _root = Directory.CreateTempSubdirectory("popote-tests-").FullName;

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        Directory.Delete(_root, recursive: true);
    }

    private string Dir(string name) => Directory.CreateDirectory(Path.Combine(_root, name)).FullName;

    // Base minimale au schéma de Popote : une recette, avec ou sans photo.
    private static void CreateDb(string path, string title, string? photoPath = null, string table = "Recipes")
    {
        using var conn = new SqliteConnection($"Data Source={path};Pooling=False");
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"CREATE TABLE {table} (Id INTEGER PRIMARY KEY, Title TEXT, PhotoPath TEXT); INSERT INTO {table} (Title, PhotoPath) VALUES ($t, $p);";
        cmd.Parameters.AddWithValue("$t", title);
        cmd.Parameters.AddWithValue("$p", (object?)photoPath ?? DBNull.Value);
        cmd.ExecuteNonQuery();
    }

    private static (string Title, string? Photo) ReadRecipe(string dbPath)
    {
        using var conn = new SqliteConnection($"Data Source={dbPath};Pooling=False");
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Title, PhotoPath FROM Recipes";
        using var r = cmd.ExecuteReader();
        r.Read();
        return (r.GetString(0), r.IsDBNull(1) ? null : r.GetString(1));
    }

    [Fact]
    public void AllerRetour_BaseEtPhotos_CheminsReecrits()
    {
        var oldPhone = Dir("ancien");
        var photo = Path.Combine(oldPhone, "recipe_abc.jpg");
        File.WriteAllBytes(photo, new byte[] { 1, 2, 3 });
        CreateDb(Path.Combine(oldPhone, BackupArchive.DbName), "Tarte", "/data/user/0/onl.nci.popote/files/recipe_abc.jpg");

        var archive = Path.Combine(_root, "sauvegarde.zip");
        using (var f = File.Create(archive))
            BackupArchive.Write(f, Path.Combine(oldPhone, BackupArchive.DbName), new[] { photo, photo, Path.Combine(oldPhone, "recipe_disparue.jpg") });

        var newPhone = Dir("nouveau");
        BackupArchive.Restore(archive, newPhone);

        var (title, path) = ReadRecipe(Path.Combine(newPhone, BackupArchive.DbName));
        Assert.Equal("Tarte", title);
        Assert.Equal(Path.Combine(newPhone, "recipe_abc.jpg"), path);
        Assert.Equal(new byte[] { 1, 2, 3 }, File.ReadAllBytes(path!));
    }

    [Fact]
    public void AncienneSauvegardeDb3_Acceptee_EtBaseActuelleGardeeDeCote()
    {
        var app = Dir("app");
        CreateDb(Path.Combine(app, BackupArchive.DbName), "Actuelle");
        var old = Path.Combine(Dir("ancien"), "popote-sauvegarde.db3");
        CreateDb(old, "Ancienne", @"C:\autre\recipe_x.png");

        BackupArchive.Restore(old, app);

        var (title, path) = ReadRecipe(Path.Combine(app, BackupArchive.DbName));
        Assert.Equal("Ancienne", title);
        Assert.Equal(Path.Combine(app, "recipe_x.png"), path);
        Assert.Equal("Actuelle", ReadRecipe(Path.Combine(app, BackupArchive.PreviousDbName)).Title);
    }

    [Fact]
    public void Undo_RemetLaBasePrecedente()
    {
        var app = Dir("app");
        CreateDb(Path.Combine(app, BackupArchive.DbName), "Actuelle");
        var old = Path.Combine(Dir("ancien"), "s.db3");
        CreateDb(old, "Ancienne");

        BackupArchive.Restore(old, app);
        BackupArchive.Undo(app);

        Assert.Equal("Actuelle", ReadRecipe(Path.Combine(app, BackupArchive.DbName)).Title);
    }

    [Theory]
    [InlineData("texte")]
    [InlineData("sqlite-sans-recettes")]
    [InlineData("zip-sans-base")]
    public void FichierInvalide_Refuse_SansToucherLaBase(string kind)
    {
        var app = Dir("app");
        var dbPath = Path.Combine(app, BackupArchive.DbName);
        CreateDb(dbPath, "Actuelle");
        var bad = Path.Combine(Dir("autre"), "mauvais");
        switch (kind)
        {
            case "texte": File.WriteAllText(bad, "pas une base"); break;
            case "sqlite-sans-recettes": CreateDb(bad, "x", table: "Autre"); break;
            case "zip-sans-base":
                using (var zip = ZipFile.Open(bad, ZipArchiveMode.Create)) zip.CreateEntry("note.txt");
                break;
        }

        Assert.Throws<InvalidDataException>(() => BackupArchive.Restore(bad, app));

        Assert.Equal("Actuelle", ReadRecipe(dbPath).Title);
        Assert.False(File.Exists(Path.Combine(app, BackupArchive.PreviousDbName)));
        Assert.Single(Directory.GetFiles(app));
    }

    [Fact]
    public void Restore_IgnoreLesEntreesHorsPhotos()
    {
        var src = Dir("src");
        CreateDb(Path.Combine(src, BackupArchive.DbName), "Tarte");
        var archive = Path.Combine(_root, "piege.zip");
        using (var zip = ZipFile.Open(archive, ZipArchiveMode.Create))
        {
            zip.CreateEntryFromFile(Path.Combine(src, BackupArchive.DbName), BackupArchive.DbName);
            zip.CreateEntry("../recipe_evasion.jpg");
            zip.CreateEntry("script.sh");
        }
        var app = Dir("app");

        BackupArchive.Restore(archive, app);

        Assert.False(File.Exists(Path.Combine(_root, "recipe_evasion.jpg")));
        Assert.False(File.Exists(Path.Combine(app, "script.sh")));
    }
}
