using RecettesApp.Views;

namespace RecettesApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // La page d'édition n'est pas dans le menu : on l'enregistre comme route
        // pour pouvoir y naviguer avec Shell.Current.GoToAsync("RecipeEditPage").
        Routing.RegisterRoute("RecipeEditPage", typeof(RecipeEditPage));
    }
}
