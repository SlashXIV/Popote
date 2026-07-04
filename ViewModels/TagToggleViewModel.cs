using CommunityToolkit.Mvvm.ComponentModel;

namespace Popote.ViewModels;

// Un tag affiché en puce à bascule : sélectionné = présent sur la recette
// (page édition) ou actif dans le filtre (page liste).
public partial class TagToggleViewModel : ObservableObject
{
    public TagToggleViewModel(string name, bool isSelected = false)
    {
        Name = name;
        IsSelected = isSelected;
    }

    public string Name { get; }

    [ObservableProperty]
    private bool isSelected;
}
