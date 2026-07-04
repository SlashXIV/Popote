using System.Text.RegularExpressions;

namespace Popote.Services;

// Découpe un texte de préparation en étapes : une ligne non vide = une étape,
// en retirant une éventuelle numérotation déjà tapée (« 1. », « 2) », « 3 - »).
public static class StepParser
{
    public static List<string> Parse(string? instructions)
        => (instructions ?? string.Empty)
            .Split('\n')
            .Select(s => StripLeadingNumber(s.Trim()))
            .Where(s => s.Length > 0)
            .ToList();

    private static string StripLeadingNumber(string line)
        => Regex.Replace(line, @"^\s*\d+\s*[.)\-–]\s*", "");
}
