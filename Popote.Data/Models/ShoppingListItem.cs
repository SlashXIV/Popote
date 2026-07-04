namespace Popote.Models;

// Un article de la liste de courses PERSISTANTE.
// - IsManual = false : généré depuis les recettes sélectionnées (remplacé à chaque « Générer »).
// - IsManual = true  : ajouté à la main (conservé lors d'un « Générer »).
// IsChecked (barré) est persisté.
public class ShoppingListItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Aisle { get; set; }
    public double Quantity { get; set; }
    public string? Unit { get; set; }
    public bool IsChecked { get; set; }
    public bool IsManual { get; set; }
}
