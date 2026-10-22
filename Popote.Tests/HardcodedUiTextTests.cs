using System.Text.RegularExpressions;

namespace Popote.Tests;

// Aucun texte d'interface en dur : tout passe par Resources/Strings/AppResources.resx.
// - XAML (Views/) : un Text, Title, Placeholder, Description ou Hint littéral contenant une lettre
//   échoue ; une valeur « {…} » (Binding, x:Static) n'est pas un littéral.
// - C# (ViewModels/, Controls/, Services/) : une chaîne littérale avec une lettre passée à une
//   alerte, une feuille d'actions, une annonce TalkBack ou une propriété affichée échoue.
// Exception : un commentaire « texte-technique : <raison> » (raison obligatoire) sur la ligne
// fautive ou celle qui la précède.
public partial class HardcodedUiTextTests
{
    [GeneratedRegex(@"\b(Text|Title|Placeholder|SemanticProperties\.Description|SemanticProperties\.Hint|Description|Eyebrow|Label|Suffix)=""(?!\{)([^""]*\p{L}[^""]*)""")]
    private static partial Regex XamlLiteral();

    [GeneratedRegex(@"(DisplayAlertAsync|DisplayActionSheetAsync|Announce)\([^;]*""[^""]*\p{L}|\b(Text|Title|Description|Placeholder)\s*=\s*\$?""[^""]*\p{L}")]
    private static partial Regex CodeLiteral();

    [GeneratedRegex(@"texte-technique\s*:\s*\S")]
    private static partial Regex Exemption();

    [Fact]
    public void Aucun_texte_en_dur_dans_les_vues()
        => AssertNone(RepoFiles.Files("Views", "*.xaml").Append(RepoFiles.PathOf("AppShell.xaml")), XamlLiteral());

    [Fact]
    public void Aucun_texte_en_dur_dans_le_code_d_interface()
        => AssertNone(new[] { "ViewModels", "Controls", "Services" }.SelectMany(d => RepoFiles.Files(d, "*.cs")), CodeLiteral());

    private static void AssertNone(IEnumerable<string> files, Regex literal)
    {
        var failures = new List<string>();
        foreach (var file in files)
        {
            var lines = File.ReadAllLines(file);
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i].TrimStart();
                if (line.StartsWith("//") || line.StartsWith("<!--") || !literal.IsMatch(lines[i])) continue;
                if (Exemption().IsMatch(lines[i]) || (i > 0 && Exemption().IsMatch(lines[i - 1]))) continue;
                failures.Add($"{Path.GetFileName(file)}:{i + 1} → {line}");
            }
        }
        Assert.True(failures.Count == 0, "Textes en dur (à passer dans AppResources.resx) :\n" + string.Join("\n", failures));
    }
}
