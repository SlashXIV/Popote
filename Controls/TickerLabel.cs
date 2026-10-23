namespace Popote.Controls;

// NumberLabel dont le texte change chaque seconde (minuteur) sans relancer la mise en page :
// Label.Text invalide la mesure jusqu'à la page (carrousel compris) à chaque changement, ce qui
// coûtait l'essentiel du CPU du décompte. Value est posé directement sur la vue native, à largeur
// figée (handler Android) : le TextView ne fait plus que se redessiner. Pour un texte de taille stable.
public class TickerLabel : NumberLabel
{
    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(string), typeof(TickerLabel), string.Empty);

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
}
