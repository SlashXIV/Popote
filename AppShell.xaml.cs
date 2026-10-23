using Popote.Motion;
using Popote.Views;

namespace Popote;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // La page d'édition n'est pas dans le menu : on l'enregistre comme route
        // pour pouvoir y naviguer avec Shell.Current.GoToAsync("RecipeEditPage").
        Routing.RegisterRoute("RecipeEditPage", typeof(RecipeEditPage));
        Routing.RegisterRoute("RecipeDetailPage", typeof(RecipeDetailPage));
        Routing.RegisterRoute("CookWithPage", typeof(CookWithPage));
        Routing.RegisterRoute("CookingModePage", typeof(CookingModePage));
        Routing.RegisterRoute("SettingsPage", typeof(SettingsPage));

        // Transitions de page : une route poussée apparaît sans animation native
        // (Shell.PresentationMode="NotAnimated" sur chaque page poussée ; un style implicite arrive
        // trop tard) puis se pose selon le niveau de mouvement.
        Application.Current!.PageAppearing += (_, page) =>
        {
            if (!_pushing || page is not ContentPage { Content: { } content }) return;
            _pushing = false;
            _ = content.RiseInAsync();
        };
    }

    private bool _pushing;

    protected override void OnNavigating(ShellNavigatingEventArgs args)
    {
        base.OnNavigating(args);
        _pushing = args.Source == ShellNavigationSource.Push;
    }
}
