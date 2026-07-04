using CommunityToolkit.Mvvm.ComponentModel;
using Popote.Models;
using Popote.Services;

namespace Popote.ViewModels;

// Un article de liste de courses avec un état « coché » (barré une fois acheté).
public partial class ShoppingItemViewModel : ObservableObject
{
    private readonly Func<int, bool, Task>? _onToggle;

    // Article éphémère (agrégation, ex. planning) — sans persistance.
    public ShoppingItemViewModel(ShoppingItem item)
    {
        Name = item.Name;
        Aisle = item.Aisle;
        QuantityLabel = item.QuantityLabel;
    }

    // Article persistant (onglet Courses) — la coche est persistée via onToggle.
    public ShoppingItemViewModel(ShoppingListItem item, Func<int, bool, Task> onToggle)
    {
        Id = item.Id;
        Name = item.Name;
        Aisle = item.Aisle ?? "Divers";
        QuantityLabel = item.Quantity <= 0
            ? string.Empty
            : (string.IsNullOrWhiteSpace(item.Unit) ? item.Quantity.ToString() : $"{item.Quantity} {item.Unit}");
        IsManual = item.IsManual;
        isChecked = item.IsChecked; // init sans notification (pas de persistance à la construction)
        _onToggle = onToggle;
    }

    public int Id { get; }
    public string Name { get; } = string.Empty;
    public string Aisle { get; } = "Divers";
    public string QuantityLabel { get; } = string.Empty;
    public bool IsManual { get; }

    [ObservableProperty]
    private bool isChecked;

    partial void OnIsCheckedChanged(bool value) => _ = _onToggle?.Invoke(Id, value);
}
