using System.Globalization;
using System.Xml.Linq;

namespace Popote.Tests;

// Relit Resources/Styles/AppColors.xaml et vérifie, pour le thème clair ET le sombre :
// - contrastes WCAG : texte 7:1, texte secondaire et accents 4,5:1, éléments graphiques 3:1 ;
// - émaux de rayon distincts deux à deux pour un œil protanope ou deutéranope
//   (simulation Machado 2009, sévérité 1, ΔE OKLab ≥ 8), puisqu'ils codent une donnée.
public class ThemeContrastTests
{
    private static readonly XNamespace X = "http://schemas.microsoft.com/winfx/2009/xaml";
    private static readonly string[] Themes = ["Light", "Dark"];
    private static readonly string[] Grounds = ["Background", "Surface", "Worktop"];
    private static readonly string[] Aisles = ["AisleProduce", "AisleDairy", "AislePantry", "AisleMeat", "AisleFrozen", "AisleOther"];

    private static readonly Dictionary<string, string> Colors = XDocument.Load(RepoFiles.PathOf("Resources", "Styles", "AppColors.xaml"))
        .Descendants().Where(e => e.Name.LocalName == "Color")
        .ToDictionary(e => (string)e.Attribute(X + "Key")!, e => e.Value.Trim());

    private static string C(string role, string theme) => Colors[role + theme];

    public static TheoryData<string> AllThemes() => [.. Themes];

    [Theory, MemberData(nameof(AllThemes))]
    public void Texte_principal_7_pour_1_sur_tous_les_fonds(string theme)
    {
        foreach (var ground in Grounds.Append("AccentSoft"))
            AssertContrast("Ink", ground, theme, 7);
    }

    [Theory, MemberData(nameof(AllThemes))]
    public void Texte_secondaire_et_accents_4_5_pour_1(string theme)
    {
        foreach (var ground in Grounds)
        {
            AssertContrast("Mute", ground, theme, 4.5);
            AssertContrast("Accent", ground, theme, 4.5);   // l'aubergine sert aussi de texte (actions, sélection)
            AssertContrast("Danger", ground, theme, 4.5);
        }
        AssertContrast("OnAccent", "Accent", theme, 4.5);
        AssertContrast("OnHighlight", "Highlight", theme, 4.5);
        AssertContrast("OnDanger", "Danger", theme, 4.5);
        AssertContrast("Accent", "AccentSoft", theme, 4.5);
    }

    [Theory, MemberData(nameof(AllThemes))]
    public void Elements_graphiques_3_pour_1(string theme)
    {
        foreach (var ground in Grounds)
        {
            AssertContrast("Favorite", ground, theme, 3);
            foreach (var aisle in Aisles)
                AssertContrast(aisle, ground, theme, 3);
        }
    }

    [Theory, MemberData(nameof(AllThemes))]
    public void Emaux_de_rayon_distincts_pour_les_daltoniens(string theme)
    {
        var failures = new List<string>();
        foreach (var (a, b) in Aisles.SelectMany((a, i) => Aisles.Skip(i + 1).Select(b => (a, b))))
            foreach (var (name, matrix) in new[] { ("protan", Protan), ("deutan", Deutan) })
            {
                var de = DeltaE(Simulate(C(a, theme), matrix), Simulate(C(b, theme), matrix));
                if (de < 8) failures.Add($"{a}/{b} {name} ΔE={de:0.0}");
            }
        Assert.True(failures.Count == 0, $"Thème {theme} : " + string.Join(", ", failures));
    }

    [Fact]
    public void Chaque_role_existe_dans_les_deux_themes()
    {
        var roles = Colors.Keys.Select(k => k.EndsWith("Light") ? k[..^5] : k.EndsWith("Dark") ? k[..^4] : k).Distinct();
        foreach (var role in roles)
            foreach (var theme in Themes)
                Assert.True(Colors.ContainsKey(role + theme), $"{role}{theme} manquant");
    }

    private static void AssertContrast(string fg, string bg, string theme, double min)
    {
        var ratio = Contrast(C(fg, theme), C(bg, theme));
        Assert.True(ratio >= min, $"{fg} sur {bg} ({theme}) : {ratio:0.00}:1 < {min}:1");
    }

    // --- WCAG 2.x ---
    private static double[] Rgb(string hex)
    {
        hex = hex.TrimStart('#');
        if (hex.Length == 8) hex = hex[2..]; // #AARRGGBB : l'alpha n'entre pas dans ces tests
        return [.. Enumerable.Range(0, 3).Select(i => int.Parse(hex.Substring(i * 2, 2), NumberStyles.HexNumber) / 255.0)];
    }

    private static double Linear(double c) => c <= 0.04045 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);

    private static double Luminance(string hex)
    {
        var c = Rgb(hex).Select(Linear).ToArray();
        return 0.2126 * c[0] + 0.7152 * c[1] + 0.0722 * c[2];
    }

    private static double Contrast(string a, string b)
    {
        var (l1, l2) = (Luminance(a), Luminance(b));
        return (Math.Max(l1, l2) + 0.05) / (Math.Min(l1, l2) + 0.05);
    }

    // --- Daltonisme simulé (Machado, Oliveira & Fernandes 2009, sévérité 1) ---
    private static readonly double[,] Protan = { { 0.152286, 1.052583, -0.204868 }, { 0.114503, 0.786281, 0.099216 }, { -0.003882, -0.048116, 1.051998 } };
    private static readonly double[,] Deutan = { { 0.367322, 0.860646, -0.227968 }, { 0.280085, 0.672501, 0.047413 }, { -0.011820, 0.042940, 0.968881 } };

    private static double[] Simulate(string hex, double[,] m)
    {
        var c = Rgb(hex).Select(Linear).ToArray();
        return [.. Enumerable.Range(0, 3).Select(i => Math.Clamp(m[i, 0] * c[0] + m[i, 1] * c[1] + m[i, 2] * c[2], 0, 1))];
    }

    // --- OKLab (Ottosson), ΔE = 100 × distance euclidienne ---
    private static double[] OkLab(double[] lin)
    {
        var l = Math.Cbrt(0.4122214708 * lin[0] + 0.5363325363 * lin[1] + 0.0514459929 * lin[2]);
        var m = Math.Cbrt(0.2119034982 * lin[0] + 0.6806995451 * lin[1] + 0.1073969566 * lin[2]);
        var s = Math.Cbrt(0.0883024619 * lin[0] + 0.2817188376 * lin[1] + 0.6299787005 * lin[2]);
        return
        [
            0.2104542553 * l + 0.7936177850 * m - 0.0040720468 * s,
            1.9779984951 * l - 2.4285922050 * m + 0.4505937099 * s,
            0.0259040371 * l + 0.7827717662 * m - 0.8086757660 * s,
        ];
    }

    private static double DeltaE(double[] linA, double[] linB)
    {
        var (a, b) = (OkLab(linA), OkLab(linB));
        return 100 * Math.Sqrt(Enumerable.Range(0, 3).Sum(i => (a[i] - b[i]) * (a[i] - b[i])));
    }
}
