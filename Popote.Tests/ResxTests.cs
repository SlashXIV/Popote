using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Popote.Tests;

// AppResources.resx et son usage restent alignés : chaque clé utilisée existe, aucune clé
// n'est orpheline, et la typographie française de base est respectée.
public partial class ResxTests
{
    private static readonly Dictionary<string, string> Strings = XDocument.Load(RepoFiles.PathOf("Resources", "Strings", "AppResources.resx"))
        .Descendants("data").ToDictionary(d => (string)d.Attribute("name")!, d => (string)d.Element("value")!);

    [GeneratedRegex(@"AppResources\.(\w+)")]
    private static partial Regex Usage();

    private static readonly HashSet<string> Used = new[] { "Views", "ViewModels", "Controls", "Services", "Converters" }
        .SelectMany(d => RepoFiles.Files(d, "*.*"))
        .Append(RepoFiles.PathOf("AppShell.xaml"))
        .Where(f => f.EndsWith(".cs") || f.EndsWith(".xaml"))
        .SelectMany(f => Usage().Matches(File.ReadAllText(f)).Select(m => m.Groups[1].Value))
        .ToHashSet();

    [Fact]
    public void Chaque_cle_utilisee_existe()
    {
        var missing = Used.Where(k => !Strings.ContainsKey(k)).ToList();
        Assert.True(missing.Count == 0, "Clés absentes du resx : " + string.Join(", ", missing));
    }

    [Fact]
    public void Aucune_cle_orpheline()
    {
        var orphans = Strings.Keys.Where(k => !Used.Contains(k)).ToList();
        Assert.True(orphans.Count == 0, "Clés jamais utilisées : " + string.Join(", ", orphans));
    }

    [Fact]
    public void Typographie_francaise()
    {
        var failures = Strings.Where(kv =>
                kv.Value.Contains('\'')                                   // apostrophe typographique ’
                || kv.Value.Contains('"')                                 // guillemets « »
                || Regex.IsMatch(kv.Value, @" [;:!?»]|« "))               // espace normale avant ; : ! ? » ou après «
            .Select(kv => $"{kv.Key} = {kv.Value}").ToList();
        Assert.True(failures.Count == 0, "Typographie :\n" + string.Join("\n", failures));
    }
}
