namespace Popote.Tests;

// Accès aux fichiers de l'app (XAML, resx, C#) depuis les tests : on remonte depuis le
// dossier de sortie jusqu'à la racine du dépôt (celle qui contient Popote.csproj).
internal static class RepoFiles
{
    public static string Root { get; } = FindRoot();

    private static string FindRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Popote.csproj")))
            dir = dir.Parent;
        return dir?.FullName ?? throw new InvalidOperationException("Racine du dépôt introuvable (Popote.csproj).");
    }

    public static string PathOf(params string[] parts) => Path.Combine([Root, .. parts]);

    public static IEnumerable<string> Files(string folder, string pattern)
        => Directory.EnumerateFiles(PathOf(folder), pattern, SearchOption.TopDirectoryOnly);
}
