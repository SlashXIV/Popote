using System.IO.Compression;
using System.Text;
using Microsoft.Data.Sqlite;

namespace Popote.Services;

// Sauvegarde complète (archive .zip : base + photos) et restauration sûre : le fichier est
// vérifié avant d'écraser quoi que ce soit, et la base actuelle est gardée de côté pour revenir
// en arrière. Accepte aussi les anciennes sauvegardes (.db3 seul).
public static class BackupArchive
{
    public const string DbName = "recettes.db3";
    public const string PreviousDbName = "recettes.avant-restauration.db3";
    private const string StagingName = "recettes.restauration.db3";

    private static readonly byte[] SqliteHeader = Encoding.ASCII.GetBytes("SQLite format 3\0");
    private static readonly byte[] ZipHeader = { (byte)'P', (byte)'K', 3, 4 };

    // Écrit l'archive : la base (déjà « checkpointée ») + les photos qui existent encore.
    public static void Write(Stream output, string dbPath, IEnumerable<string> photoPaths)
    {
        using var zip = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true);
        zip.CreateEntryFromFile(dbPath, DbName);
        foreach (var photo in photoPaths.Distinct().Where(File.Exists))
            zip.CreateEntryFromFile(photo, Path.GetFileName(photo), CompressionLevel.NoCompression); // JPEG déjà compressé
    }

    // Vrai si le fichier est une base SQLite contenant la table des recettes.
    public static bool IsValidDatabase(string path)
    {
        if (!StartsWith(path, SqliteHeader)) return false;
        try
        {
            using var conn = new SqliteConnection(ConnectionString(path, SqliteOpenMode.ReadOnly));
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT count(*) FROM sqlite_master WHERE type = 'table' AND name = 'Recipes'";
            return Convert.ToInt64(cmd.ExecuteScalar()) == 1;
        }
        catch (SqliteException)
        {
            return false;
        }
    }

    // Remplace la base de appDir par celle de la sauvegarde (.zip ou .db3) et extrait les photos.
    // Lève InvalidDataException si le fichier n'est pas une sauvegarde Popote (rien n'est touché).
    // La base actuelle est copiée en PreviousDbName ; Undo la remet en place.
    public static void Restore(string source, string appDir)
    {
        var staging = Path.Combine(appDir, StagingName);
        var isZip = StartsWith(source, ZipHeader);
        try
        {
            if (isZip)
            {
                using var zip = ZipFile.OpenRead(source);
                var db = zip.GetEntry(DbName) ?? throw new InvalidDataException("Archive sans base.");
                db.ExtractToFile(staging, overwrite: true);
            }
            else
            {
                File.Copy(source, staging, overwrite: true);
            }

            if (!IsValidDatabase(staging)) throw new InvalidDataException("Ce fichier n'est pas une sauvegarde Popote.");
            RelinkPhotos(staging, appDir);
        }
        catch
        {
            File.Delete(staging);
            throw;
        }

        var dbPath = Path.Combine(appDir, DbName);
        SqliteConnection.ClearAllPools(); // les connexions en cache pointeraient sur l'ancien fichier
        if (File.Exists(dbPath)) File.Copy(dbPath, Path.Combine(appDir, PreviousDbName), overwrite: true);
        try
        {
            DeleteJournals(dbPath);
            File.Move(staging, dbPath, overwrite: true);
            if (isZip) ExtractPhotos(source, appDir);
        }
        catch
        {
            Undo(appDir);
            throw;
        }
    }

    // Remet la base d'avant la restauration (après un échec, y compris de migration).
    public static void Undo(string appDir)
    {
        var previous = Path.Combine(appDir, PreviousDbName);
        if (!File.Exists(previous)) return;
        var dbPath = Path.Combine(appDir, DbName);
        SqliteConnection.ClearAllPools();
        DeleteJournals(dbPath);
        File.Copy(previous, dbPath, overwrite: true);
    }

    // Chemin de la photo une fois rapatriée dans le dossier de l'app (seul le nom de fichier compte).
    public static string LocalPhotoPath(string photoPath, string appDir)
        => Path.Combine(appDir, Path.GetFileName(photoPath.Replace('\\', '/')));

    private static bool IsPhotoName(string name)
        => name.StartsWith("recipe_", StringComparison.Ordinal)
           && name == Path.GetFileName(name) // pas de sous-dossier ni de « .. » dans l'archive
           && (name.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
               || name.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
               || name.EndsWith(".png", StringComparison.OrdinalIgnoreCase));

    private static void ExtractPhotos(string source, string appDir)
    {
        using var zip = ZipFile.OpenRead(source);
        foreach (var entry in zip.Entries.Where(e => IsPhotoName(e.FullName)))
            entry.ExtractToFile(Path.Combine(appDir, entry.FullName), overwrite: true);
    }

    // PhotoPath est un chemin absolu, propre à l'appareil d'origine : on le refait pointer ici.
    private static void RelinkPhotos(string dbPath, string appDir)
    {
        using var conn = new SqliteConnection(ConnectionString(dbPath, SqliteOpenMode.ReadWrite));
        conn.Open();
        var rows = new List<(long Id, string Path)>();
        using (var read = conn.CreateCommand())
        {
            read.CommandText = "SELECT Id, PhotoPath FROM Recipes WHERE PhotoPath IS NOT NULL AND PhotoPath <> ''";
            using var r = read.ExecuteReader();
            while (r.Read()) rows.Add((r.GetInt64(0), r.GetString(1)));
        }
        using var tx = conn.BeginTransaction();
        foreach (var (id, path) in rows)
        {
            using var update = conn.CreateCommand();
            update.Transaction = tx;
            update.CommandText = "UPDATE Recipes SET PhotoPath = $p WHERE Id = $id";
            update.Parameters.AddWithValue("$p", LocalPhotoPath(path, appDir));
            update.Parameters.AddWithValue("$id", id);
            update.ExecuteNonQuery();
        }
        tx.Commit();
    }

    private static void DeleteJournals(string dbPath)
    {
        foreach (var ext in new[] { "-wal", "-shm" })
            File.Delete(dbPath + ext);
    }

    private static bool StartsWith(string path, byte[] magic)
    {
        using var f = File.OpenRead(path);
        var head = new byte[magic.Length];
        return f.ReadAtLeast(head, head.Length, throwOnEndOfStream: false) == head.Length && head.AsSpan().SequenceEqual(magic);
    }

    // Sans pool : le fichier doit pouvoir être remplacé juste après.
    private static string ConnectionString(string path, SqliteOpenMode mode)
        => new SqliteConnectionStringBuilder { DataSource = path, Mode = mode, Pooling = false }.ToString();
}
