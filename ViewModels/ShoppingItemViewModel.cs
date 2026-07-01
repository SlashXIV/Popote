using CommunityToolkit.Mvvm.ComponentModel;
using Popote.Services;

namespace Popote.ViewModels;

// Un article de la liste de courses avec un état « coché » (barré une fois acheté).
// L'état est éphémère (le temps des courses) : régénérer la liste le réinitialise.
public partial class ShoppingItemViewModel : ObservableObject
{
    public ShoppingItemViewModel(ShoppingItem item)
    {
        Name = item.Name;
        Aisle = item.Aisle;
        QuantityLabel = item.QuantityLabel;
    }

    public string Name { get; }
    public string Aisle { get; }
    public string QuantityLabel { get; }

    [ObservableProperty]
    private bool isChecked;
}
