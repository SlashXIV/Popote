namespace Popote.Controls;

// Contrôle maison activable par TalkBack (double toucher) : le délégué d'accessibilité
// Android appelle Activate(), qui fait exactement ce que fait le toucher.
public interface IActivatable
{
    void Activate();
}
